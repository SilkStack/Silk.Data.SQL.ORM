using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.NewModelling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class InsertQueryBuilder<TSource>
		where TSource : new()
	{
		public EntitySchema<TSource> EntitySchema { get; }

		public InsertQueryBuilder(EntitySchema<TSource> entitySchema)
		{
			EntitySchema = entitySchema;
		}

		public EntityModel<TSource> DataModel { get; }

		[Obsolete]
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
			return CreateQuery(EntitySchema, sources);
		}

		public ICollection<ORMQuery> CreateQuery<TView>(IEntitySchema<TView> schema, IEnumerable<TView> sources)
			where TView : new()
		{
			if (sources == null)
				throw new ArgumentNullException(nameof(sources));

			var sourceReadWriters = sources
				.Select(q => new ObjectModelReadWriter(schema.Model, q))
				.ToArray();

			var queries = new List<ORMQuery>();

			var bulkInserts = CreateBulkInsertQuery(schema, sourceReadWriters);
			if (bulkInserts != null)
				queries.Add(bulkInserts);

			return queries;
		}

		private ORMQuery CreateBulkInsertQuery<TView>(IEntitySchema<TView> schema, ObjectModelReadWriter[] sources)
			where TView : new()
		{
			List<QueryExpression[]> rows = null;

			foreach (var source in sources)
			{
				var row = new List<QueryExpression>(schema.Fields.Length);
				var rowIsAutoIncrement = false;

				foreach (var field in schema.Fields)
				{
					if (field.IsMappedObject)
						continue;

					var modelValue = field.ModelBinding.ReadValue<object>(source);

					if (field.AutoGenerate)
					{
						if (field.IsAutoIncrement && ValueIsDefault(field, modelValue))
						{
							rowIsAutoIncrement = true;
							break;
						}

						if (field.DataType == typeof(Guid) && ValueIsDefault(field, modelValue))
						{
							var newId = Guid.NewGuid();
							field.ModelBinding.WriteValue(source, newId);
							modelValue = newId;
						}
					}

					row.Add(QueryExpression.Value(
						modelValue
						));
				}

				if (!rowIsAutoIncrement)
				{
					if (rows == null)
						rows = new List<QueryExpression[]>();
					rows.Add(row.ToArray());
				}
			}

			if (rows == null)
				return null;

			return new NoResultORMQuery(
				QueryExpression.Insert(
					schema.EntityTable.TableName,
					schema.Fields
						.Where(q => !q.IsMappedObject)
						.Select(q => q.Name).ToArray(),
					rows.ToArray()
				));
		}

		private bool ValueIsDefault(IDataField field, object value)
		{
			if (field.DataType == typeof(short))
				return (short)value == 0;
			if (field.DataType == typeof(ushort))
				return (ushort)value == 0;
			if (field.DataType == typeof(int))
				return (int)value == 0;
			if (field.DataType == typeof(uint))
				return (uint)value == 0;
			if (field.DataType == typeof(long))
				return (long)value == 0;
			if (field.DataType == typeof(ulong))
				return (ulong)value == 0;
			if (field.DataType == typeof(Guid))
				return (Guid)value == default(Guid);
			return false;
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
						if (rows == null)
							rows = new List<QueryExpression[]>();
						else
							rows.Clear();

						var joinTable = schema.Tables.FirstOrDefault(q => q.IsJoinTableFor(model.Schema.EntityTable.EntityType, field.Relationship.ForeignModel.EntityType));
						if (joinTable == null)
							throw new InvalidOperationException($"Couldn't locate join table for '{field.Relationship.ForeignModel.EntityType.FullName}'.");

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
								else if (joinTable.DataFields[i].RelatedEntityType == field.Relationship.ForeignModel.EntityType)
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
