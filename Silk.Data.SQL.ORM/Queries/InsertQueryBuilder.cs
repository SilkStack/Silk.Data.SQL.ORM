using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.NewModelling;
using System;
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

		public ICollection<ORMQuery> CreateQuery<TView>(params TView[] sources)
			where TView : new()
		{
			return CreateQuery(sources as IEnumerable<TView>);
		}

		public ICollection<ORMQuery> CreateQuery<TView>(IEnumerable<TView> sources)
			where TView : new()
		{
			var projectionModel = EntitySchema.GetProjection<TView>();

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

			var autoIncInserts = CreateAutoIncrementQueries(schema, sourceReadWriters);
			if (autoIncInserts != null)
				queries.AddRange(autoIncInserts);

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

					if (field.IsRelationshipKey)
					{
						row.Add(new LateReadValueExpression(() =>
							field.ModelBinding.ReadValue<object>(source)
							));
					}
					else
					{
						row.Add(QueryExpression.Value(
							modelValue
							));
					}
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

		private ICollection<ORMQuery> CreateAutoIncrementQueries<TView>(IEntitySchema<TView> schema, ObjectModelReadWriter[] sources)
			where TView : new()
		{
			var autoIncField = schema.EntityTable.PrimaryKeyFields.FirstOrDefault(q => q.IsAutoIncrement);
			if (autoIncField == null)
				return null;

			List<ORMQuery> queries = null;

			foreach (var source in sources)
			{
				var rowIsAutoIncrement = false;
				var row = new List<QueryExpression>();

				foreach (var field in schema.Fields)
				{
					if (field.IsMappedObject)
						continue;

					var modelValue = field.ModelBinding.ReadValue<object>(source);

					if (field.IsAutoIncrement)
					{
						rowIsAutoIncrement = ValueIsDefault(field, modelValue);
						if (!rowIsAutoIncrement)
							break;

						continue;
					}

					if (field.IsRelationshipKey)
					{
						row.Add(new LateReadValueExpression(() =>
							field.ModelBinding.ReadValue<object>(source)
							));
					}
					else
					{
						row.Add(QueryExpression.Value(
							modelValue
							));
					}
				}

				if (rowIsAutoIncrement)
				{
					if (queries == null)
						queries = new List<ORMQuery>();

					queries.Add(new NoResultORMQuery(
						QueryExpression.Insert(
							schema.EntityTable.TableName,
							schema.Fields
								.Where(q => !q.IsMappedObject && !q.IsAutoIncrement)
								.Select(q => q.Name).ToArray(),
							row.ToArray()
						)));

					queries.Add(new AssignAutoIncrementORMQuery(
						QueryExpression.Select(new[] {
							QueryExpression.LastInsertIdFunction()
						}), autoIncField.DataType, autoIncField, source));
				}
			}

			return queries;
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
	}
}
