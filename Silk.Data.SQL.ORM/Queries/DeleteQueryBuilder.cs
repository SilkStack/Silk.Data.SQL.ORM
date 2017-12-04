using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DeleteQueryBuilder<TSource>
		where TSource : new()
	{
		public EntityModel<TSource> DataModel { get; }

		public DeleteQueryBuilder(EntityModel<TSource> dataModel)
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

			QueryExpression whereExpr = null;
			foreach (var sourceReadWriter in sourceReadWriters)
			{
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
				if (whereExpr == null)
					whereExpr = sourceWhere;
				else
					whereExpr = QueryExpression.OrElse(whereExpr, sourceWhere);
			}

			return new ORMQuery[]
			{
				new NoResultORMQuery(
					QueryExpression.Delete(
						QueryExpression.Table(DataModel.Schema.EntityTable.TableName),
						whereConditions: whereExpr
					))
			};
		}

		public ICollection<ORMQuery> CreateQuery(
			QueryExpression where = null
			)
		{
			//  todo: update this to work with datamodels that span multiple tables
			return new ORMQuery[]
			{
				new NoResultORMQuery(
					QueryExpression.Delete(
						QueryExpression.Table(DataModel.Schema.EntityTable.TableName),
						whereConditions: where
					))
			};
		}
	}
}
