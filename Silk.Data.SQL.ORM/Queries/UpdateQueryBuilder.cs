using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class UpdateQueryBuilder<TSource>
		where TSource : new()
	{
		public EntityModel<TSource> DataModel { get; }

		public UpdateQueryBuilder(EntityModel<TSource> dataModel)
		{
			DataModel = dataModel;
		}

		public ICollection<ORMQuery> CreateQuery(params TSource[] sources)
		{
			if (sources == null || sources.Length < 1)
				throw new ArgumentException("At least one source must be provided.", nameof(sources));

			if (DataModel.PrimaryKeyFields == null ||
				DataModel.PrimaryKeyFields.Length == 0)
				throw new InvalidOperationException("A primary key is required.");

			var sourceReadWriters = sources
				.Select(q => new ObjectModelReadWriter(DataModel.Model, q))
				.ToArray();

			var queries = new List<ORMQuery>();
			foreach (var sourceReadWriter in sourceReadWriters)
			{
				var row = new Dictionary<DataField,QueryExpression>();
				foreach (var field in DataModel.Fields)
				{
					row.Add(field, QueryExpression.Value(
						field.ModelBinding.ReadValue<object>(sourceReadWriter)
						));
				}

				QueryExpression sourceWhere = null;
				foreach (var primaryKey in DataModel.PrimaryKeyFields)
				{
					var pkCondition = QueryExpression.Compare(
						QueryExpression.Column(primaryKey.Storage.ColumnName),
						ComparisonOperator.AreEqual,
						QueryExpression.Value(primaryKey.ModelBinding.ReadValue<object>(sourceReadWriter))
						);

					if (sourceWhere == null)
						sourceWhere = pkCondition;
					else
						sourceWhere = QueryExpression.AndAlso(sourceWhere, pkCondition);
				}

				queries.Add(new NoResultORMQuery(
					QueryExpression.Update(
						QueryExpression.Table(DataModel.Schema.EntityTable.TableName),
						where: sourceWhere,
						assignments: row.Select(kvp => QueryExpression.Assign(
							QueryExpression.Column(kvp.Key.Storage.ColumnName),
							kvp.Value
							)).ToArray()
					)));
			}
			return queries;
		}
	}
}
