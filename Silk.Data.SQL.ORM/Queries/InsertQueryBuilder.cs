using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class InsertQueryBuilder<TSource>
		where TSource : new()
	{
		public EntityModel<TSource> DataModel { get; }

		public InsertQueryBuilder(EntityModel<TSource> dataModel)
		{
			DataModel = dataModel;
		}

		public ICollection<ORMQuery> CreateQuery<TView>(params TView[] sources)
			where TView : new()
		{
			var projectionModel = DataModel.Domain
				.GetProjectionModel<TSource, TView>();

			return CreateQuery(projectionModel, sources);
		}

		public ICollection<ORMQuery> CreateQuery(params TSource[] sources)
		{
			return CreateQuery(DataModel, sources);
		}

		public ICollection<ORMQuery> CreateQuery<TView>(EntityModel<TView> model, params TView[] sources)
			where TView : new()
		{
			if (sources == null || sources.Length < 1)
				throw new ArgumentException("At least one source must be provided.", nameof(sources));

			var queries = new List<ORMQuery>();
			var autoIncField = DataModel.PrimaryKeyFields.FirstOrDefault(q => q.Storage.IsAutoIncrement);
			var isBulkInsert = autoIncField == null;
			List<QueryExpression[]> rows = null;
			if (isBulkInsert)
				rows = new List<QueryExpression[]>();

			var sourceReadWriters = sources
				.Select(q => new ObjectModelReadWriter(model.Model, q))
				.ToArray();

			foreach (var sourceReadWriter in sourceReadWriters)
			{
				var row = new List<QueryExpression>();
				foreach (var field in model.Fields)
				{
					if (field.Storage.IsAutoIncrement)
					{
						continue;
					}

					if (field.Storage.IsAutoGenerate &&
						field.DataType == typeof(Guid))
					{
						field.ModelBinding.WriteValue(sourceReadWriter, Guid.NewGuid());
					}

					if (field.Relationship == null)
					{
						row.Add(QueryExpression.Value(
							field.ModelBinding.ReadValue<object>(sourceReadWriter)
							));
					}
					else
					{
						row.Add(new LateReadValueExpression(() =>
							field.ModelBinding.ReadValue<object>(sourceReadWriter)
							));
					}
				}

				if (isBulkInsert)
				{
					rows.Add(row.ToArray());
				}
				else
				{
					queries.Add(new NoResultORMQuery(
						QueryExpression.Insert(
							DataModel.Schema.EntityTable.TableName,
							model.Fields
								.Where(q => q.Storage.Table.IsEntityTable && !q.Storage.IsAutoIncrement)
								.Select(q => q.Storage.ColumnName).ToArray(),
							row.ToArray()
						)));

					queries.Add(new AssignAutoIncrementORMQuery(
						QueryExpression.Select(new[] {
							QueryExpression.LastInsertIdFunction()
						}), autoIncField.DataType, autoIncField, sourceReadWriter));
				}
			}

			if (isBulkInsert)
			{
				queries.Add(new NoResultORMQuery(
					QueryExpression.Insert(
						DataModel.Schema.EntityTable.TableName,
						model.Fields
							.Where(q => q.Storage.Table.IsEntityTable)
							.Select(q => q.Storage.ColumnName).ToArray(),
						rows.ToArray()
					)));
			}

			return queries;
		}
	}
}
