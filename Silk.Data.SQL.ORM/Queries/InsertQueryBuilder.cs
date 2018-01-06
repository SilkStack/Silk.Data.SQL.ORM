using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections;
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
			return CreateQuery(sources as IEnumerable<TView>);
		}

		public ICollection<ORMQuery> CreateQuery<TView>(IEnumerable<TView> sources)
			where TView : new()
		{
			var projectionModel = DataModel.Domain
				.GetProjectionModel<TSource, TView>();

			return CreateQuery(projectionModel, sources);
		}

		public ICollection<ORMQuery> CreateQuery(params TSource[] sources)
		{
			return CreateQuery(sources as IEnumerable<TSource>);
		}

		public ICollection<ORMQuery> CreateQuery(IEnumerable<TSource> sources)
		{
			return CreateQuery(DataModel, sources);
		}

		private ICollection<ORMQuery> CreateQuery<TView>(EntityModel<TView> model, IEnumerable<TView> sources)
			where TView : new()
		{
			if (sources == null)
				throw new ArgumentNullException(nameof(sources));

			var schema = model.Schema;
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
				foreach (var field in model.Fields.Where(q => q.Storage != null))
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
								.Where(q => q.Storage?.Table.IsEntityTable == true && q.Storage?.IsAutoIncrement == false)
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
							.Where(q => q.Storage?.Table.IsEntityTable == true)
							.Select(q => q.Storage.ColumnName).ToArray(),
						rows.ToArray()
					)));
			}

			var manyToManyFields = model.Fields
				.Where(q => q.Storage == null && q.Relationship != null && q.Relationship.RelationshipType == RelationshipType.ManyToMany)
				.ToArray();
			if (manyToManyFields.Length > 0)
			{
				foreach (var sourceReadWriter in sourceReadWriters)
				{
					foreach (var field in manyToManyFields)
					{
						rows.Clear();

						var joinTable = schema.Tables.FirstOrDefault(q => q.IsJoinTableFor(model.Schema.EntityTable.EntityType, field.DataType));
						if (joinTable == null)
							throw new InvalidOperationException($"Couldn't locate join table for '{field.DataType.FullName}'.");

						var entityKeyFields = joinTable.DataFields.Where(q => q.RelatedEntityType == model.EntityType).ToArray();
						var valueKeyFields = joinTable.DataFields.Where(q => q.RelatedEntityType == field.DataType).ToArray();

						var valueEnum = field.ModelBinding.ReadValue<object>(sourceReadWriter) as IEnumerable;
						if (valueEnum == null)
							continue;

						foreach (var value in valueEnum)
						{
							var valueReadWriter = new ObjectModelReadWriter(field.Relationship.ForeignModel.Model, value);
							var row = new QueryExpression[joinTable.DataFields.Count];

							for (var i = 0; i < row.Length; i++)
							{
								var dataField = joinTable.DataFields[i];
								if (joinTable.DataFields[i].RelatedEntityType == model.Schema.EntityTable.EntityType)
								{
									row[i] = new LateReadValueExpression(() =>
									{
										return dataField.ModelBinding.ReadValue<object>(sourceReadWriter);
									});
								}
								else if (joinTable.DataFields[i].RelatedEntityType == field.DataType)
								{
									row[i] = new LateReadValueExpression(() =>
									{
										return dataField.ModelBinding.ReadValue<object>(valueReadWriter);
									});
								}
								else
								{
									row[i] = new LateReadValueExpression(() =>
									{
										return null;
									});
								}
							}

							rows.Add(row);
						}

						queries.Add(new NoResultORMQuery(
							QueryExpression.Insert(
								joinTable.TableName,
								joinTable.DataFields
									.Select(q => q.Storage.ColumnName).ToArray(),
								rows.ToArray()
							)));
					}
				}
			}

			return queries;
		}
	}
}
