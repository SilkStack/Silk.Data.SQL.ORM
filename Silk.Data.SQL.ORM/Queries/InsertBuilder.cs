using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class InsertBuilder : IQueryBuilder
	{
		protected string Into { get; set; }
		protected bool HasServerGeneratedPrimaryKey { get; }
		protected string[] Columns { get; }
		protected List<IValueReader[]> RowValueReaders { get; }
			= new List<IValueReader[]>();

		public InsertBuilder(string[] columns, bool hasServerGeneratedPrimaryKey)
		{
			Columns = columns;
			HasServerGeneratedPrimaryKey = hasServerGeneratedPrimaryKey;
		}

		public QueryExpression BuildQuery()
		{
			if (!HasServerGeneratedPrimaryKey)
			{
				return QueryExpression.Insert(
					Into,
					Columns,
					RowValueReaders.SelectMany(
						row => row.Select(reader => QueryExpression.Value(reader.Read()) as QueryExpression)
						).ToArray()
					);
			}
			else
			{
				return new CompositeQueryExpression(GetInsertAndSelectIdentityQueries());
			}
		}

		private IEnumerable<QueryExpression> GetInsertAndSelectIdentityQueries()
		{
			foreach (var row in RowValueReaders)
			{
				yield return QueryExpression.Insert(
					Into,
					Columns,
					row.Select(reader => QueryExpression.Value(reader.Read()) as QueryExpression).ToArray()
					);
				yield return QueryExpression.Select(
					QueryExpression.Alias(QueryExpression.LastInsertIdFunction(), "__PK_IDENTITY")
					);
			}
		}
	}

	public class InsertBuilder<T> : InsertBuilder
	{
		protected static TypeModel<T> Model { get; }
			= TypeModel.GetModelOf<T>();

		public InsertBuilder(string[] columns, bool hasServerGeneratedPrimaryKey)
			: base(columns, hasServerGeneratedPrimaryKey)
		{
		}
	}

	public class EntityInsertBuilder<T> : InsertBuilder<T>
		where T : class
	{
		public Schema.Schema Schema { get; }
		public EntitySchema<T> EntitySchema { get; }

		public EntityInsertBuilder(Schema.Schema schema)
			: base(
				  schema.GetEntitySchema<T>().EntityFields
					.Where(q => q.PrimaryKeyGenerator != PrimaryKeyGenerator.ServerGenerated)
					.SelectMany(q => q.Columns)
					.Select(q => q.ColumnName).ToArray(),
				  schema.GetEntitySchema<T>().EntityFields
					.Any(q => q.PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated)
				  )
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
			Into = EntitySchema.EntityTable.TableName;
		}

		private void Add(T instance)
		{
			var row = new List<IValueReader>();
			foreach (var field in EntitySchema.EntityFields
				.Where(q => q.PrimaryKeyGenerator != PrimaryKeyGenerator.ServerGenerated))
			{
				foreach (var column in field.Columns)
				{
					if (field.PrimaryKeyGenerator == PrimaryKeyGenerator.ClientGenerated)
					{
						var newId = Guid.NewGuid();
						//  write generated ID to the object directly so that following queries can reference it
						var readWriter = new ObjectReadWriter(instance, Model, typeof(T));
						readWriter.WriteField<Guid>(field.ModelPath, 0, newId);
					}
					row.Add(field.GetValueReader(instance, column));
				}
			}
			RowValueReaders.Add(row.ToArray());
		}

		private IEnumerable<Binding> CreateMappingBindings()
		{
			foreach (var field in EntitySchema.EntityFields
				.Where(q => q.PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated))
			{
				yield return field.GetValueBinding();
			}
		}

		private ObjectResultMapper<T> CreateResultMapper(int resultSetCount)
		{
			return new ObjectResultMapper<T>(resultSetCount, CreateMappingBindings());
		}

		public ObjectResultMapper<T> Add(IEnumerable<T> instances)
		{
			var instanceCount = 0;
			foreach (var instance in instances)
			{
				Add(instance);
				instanceCount++;
			}
			if (!HasServerGeneratedPrimaryKey)
				return null;
			return CreateResultMapper(instanceCount);
		}

		public ObjectResultMapper<T> Add(params T[] instances) => Add((IEnumerable<T>)instances);
	}
}
