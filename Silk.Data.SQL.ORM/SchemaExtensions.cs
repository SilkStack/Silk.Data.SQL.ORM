using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Queries.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM
{
	public static class SchemaExtensions
	{
		private static void SanityCheckArgs<T>(EntitySchema<T> schema, T[] entities, bool primaryKeyRequired = true)
			where T : class
		{
			if (entities == null || entities.Length < 1)
				throw new Exception("At least 1 entity must be provided.");

			if (primaryKeyRequired && !schema.SchemaFields.Any(q => q.IsPrimaryKey))
				throw new Exception("Entity type must have a primary key to generate update statements.");
		}

		public static QueryNoResult CreateInsert<T>(this EntitySchema<T> schema, Action<EntityInsertBuilder<T>> queryCallback)
			where T : class
		{
			var builder = new EntityInsertBuilder<T>(schema);
			queryCallback?.Invoke(builder);
			return new QueryNoResult(builder.BuildQuery());
		}

		public static QueryNoResult CreateInsert<T>(this Schema.Schema schema, Action<EntityInsertBuilder<T>> queryCallback)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
			return entitySchema.CreateInsert<T>(queryCallback);
		}

		public static QueryNoResult CreateUpdate<T>(this EntitySchema<T> schema, Action<EntityUpdateBuilder<T>> queryCallback)
			where T : class
		{
			var builder = new EntityUpdateBuilder<T>(schema);
			queryCallback?.Invoke(builder);
			return new QueryNoResult(builder.BuildQuery());
		}

		public static QueryNoResult CreateUpdate<T>(this Schema.Schema schema, Action<EntityUpdateBuilder<T>> queryCallback)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
			return entitySchema.CreateUpdate<T>(queryCallback);
		}

		public static QueryNoResult CreateDelete<T>(this EntitySchema<T> schema, Action<EntityDeleteBuilder<T>> queryCallback)
			where T : class
		{
			var builder = new EntityDeleteBuilder<T>(schema);
			queryCallback?.Invoke(builder);
			return new QueryNoResult(builder.BuildQuery());
		}

		public static QueryNoResult CreateDelete<T>(this Schema.Schema schema, Action<EntityDeleteBuilder<T>> queryCallback)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
			return entitySchema.CreateDelete<T>(queryCallback);
		}

		public static QueryWithScalarResult<bool> CreateTableExists<T>(this EntitySchema<T> schema)
			where T : class
		{
			return new QueryWithScalarResult<bool>(
				QueryExpression.TableExists(schema.EntityTable.TableName),
				new ValueResultMapper<bool>(1, OrdinalFieldReference<bool>.Create(0))
				);
		}

		public static QueryWithScalarResult<bool> CreateTableExists<T>(this Schema.Schema schema)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateTableExists();
		}

		public static QueryNoResult CreateTable<T>(this EntitySchema<T> schema)
			where T : class
		{
			var queryBuilder = new CreateEntityTableBuilder<T>(schema);
			return new QueryNoResult(queryBuilder.BuildQuery());
		}

		public static QueryNoResult CreateTable<T>(this Schema.Schema schema)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateTable();
		}

		public static QueryWithScalarResult<int> CreateCount<T>(this EntitySchema<T> schema, Action<EntitySelectBuilder<T>> queryCallback = null)
			where T : class
		{
			var queryBuilder = new EntitySelectBuilder<T>(schema);
			var mapping = queryBuilder.Project(q => DatabaseFunctions.Count(q));
			queryCallback?.Invoke(queryBuilder);
			return new QueryWithScalarResult<int>(queryBuilder.BuildQuery(), mapping);
		}

		public static QueryWithScalarResult<int> CreateCount<T>(this Schema.Schema schema, Action<EntitySelectBuilder<T>> queryCallback = null)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateCount(queryCallback);
		}

		public static QueryWithMappedResult<T> CreateSelect<T>(this EntitySchema<T> schema, Action<EntitySelectBuilder<T>> queryCallback = null)
			where T : class
		{
			var queryBuilder = new EntitySelectBuilder<T>(schema);
			var mapping = queryBuilder.Project<T>();
			queryCallback?.Invoke(queryBuilder);
			return new QueryWithMappedResult<T>(queryBuilder.BuildQuery(), mapping);
		}

		public static QueryWithMappedResult<T> CreateSelect<T>(this Schema.Schema schema, Action<EntitySelectBuilder<T>> queryCallback = null)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateSelect(queryCallback);
		}

		public static QueryWithMappedResult<TView> CreateSelect<T, TView>(this EntitySchema<T> schema, Action<EntitySelectBuilder<T>> queryCallback = null)
			where T : class
			where TView : class
		{
			var queryBuilder = new EntitySelectBuilder<T>(schema);
			var mapping = queryBuilder.Project<TView>();
			queryCallback?.Invoke(queryBuilder);
			return new QueryWithMappedResult<TView>(queryBuilder.BuildQuery(), mapping);
		}

		public static QueryWithMappedResult<TView> CreateSelect<T, TView>(this Schema.Schema schema, Action<EntitySelectBuilder<T>> queryCallback = null)
			where T : class
			where TView : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateSelect<T, TView>(queryCallback);
		}

		public static Query CreateInsert<T>(this EntitySchema<T> schema, params T[] entities)
			where T : class
		{
			SanityCheckArgs(schema, entities, primaryKeyRequired: false);

			var entityTypeModel = TypeModel.GetModelOf<T>();
			var serverGeneratedPrimaryKeyField = schema.SchemaFields.FirstOrDefault(q => q.PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated);
			var isBulkInsert = serverGeneratedPrimaryKeyField == null;

			if (isBulkInsert)
				return new QueryNoResult(
					new CompositeQueryExpression(BuildExpressions())
					);

			return new QueryInjectResult<T>(
				new CompositeQueryExpression(BuildExpressions()),
				new ObjectResultMapper<T>(
					entities.Length,
					new Mapping(
						null, null,
						schema.SchemaFields.Where(q => q.PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated)
							.SelectMany(q => q.Bindings).ToArray()
					)),
				entities
				);

			IEnumerable<QueryExpression> BuildExpressions()
			{
				var entityReadWriter = new ObjectReadWriter(null, entityTypeModel, typeof(T));
				var queryBuilder = default(EntityInsertBuilder<T>);
				if (isBulkInsert)
					queryBuilder = new EntityInsertBuilder<T>(schema, entityReadWriter);

				foreach (var entity in entities)
				{
					if (!isBulkInsert)
						queryBuilder = new EntityInsertBuilder<T>(schema, entityReadWriter);
					else
						queryBuilder.NewRow();

					foreach (var field in schema.SchemaFields.Where(q => q.FieldType != FieldType.JoinedField))
					{
						if (field.PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated)
							continue;

						if (field.PrimaryKeyGenerator == PrimaryKeyGenerator.ClientGenerated)
						{
							var newId = Guid.NewGuid();
							//  write generated ID to the object directly so that following queries can reference it
							entityReadWriter.WriteField(entityTypeModel.Root, entity);
							entityReadWriter.WriteField(field.EntityFieldReference, newId);
						}

						queryBuilder.Set(field, entity);
					}

					if (!isBulkInsert)
					{
						yield return queryBuilder.BuildQuery();
						//  todo: support composite primary keys?
						var selectPKQueryBuilder = new EntitySelectBuilder<T>(schema.Schema);
						selectPKQueryBuilder.Project<int>(QueryExpression.Alias(
							QueryExpression.LastInsertIdFunction(),
							schema.SchemaFields.First(q => q.PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated).AliasName
							));
						yield return selectPKQueryBuilder.BuildQuery();
					}
				}

				if (isBulkInsert)
					yield return queryBuilder.BuildQuery();
			}
		}

		public static Query CreateInsert<T>(this Schema.Schema schema, params T[] entities)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateInsert(entities);
		}

		public static QueryNoResult CreateDelete<T>(this EntitySchema<T> schema, params T[] entities)
			where T : class
		{
			SanityCheckArgs(schema, entities);

			var entityReadWriter = new ObjectReadWriter(null, TypeModel.GetModelOf<T>(), typeof(T));

			return new QueryNoResult(
				new CompositeQueryExpression(BuildExpressions())
				);

			IEnumerable<QueryExpression> BuildExpressions()
			{
				foreach (var entity in entities)
				{
					var queryBuilder = new EntityDeleteBuilder<T>(schema, entityReadWriter);
					foreach (var field in schema.SchemaFields)
					{
						if (field.IsPrimaryKey)
							queryBuilder.AndWhere(field, ComparisonOperator.AreEqual, entity);
					}
					yield return queryBuilder.BuildQuery();
				}
			}
		}

		public static QueryNoResult CreateDelete<T>(this Schema.Schema schema, params T[] entities)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateDelete(entities);
		}

		public static QueryNoResult CreateUpdate<T>(this EntitySchema<T> schema, params T[] entities)
			where T : class
		{
			SanityCheckArgs(schema, entities);

			var entityReadWriter = ObjectReadWriter.Create<T>();
			return new QueryNoResult(
				new CompositeQueryExpression(BuildExpressions())
				);

			IEnumerable<QueryExpression> BuildExpressions()
			{
				foreach (var entity in entities)
				{
					var queryBuilder = new EntityUpdateBuilder<T>(schema);
					foreach (var field in schema.SchemaFields)
					{
						if (field.IsPrimaryKey)
							queryBuilder.AndWhere(field, ComparisonOperator.AreEqual, entity);
						else
							queryBuilder.Set(field, entity);
					}
					yield return queryBuilder.BuildQuery();
				}
			}
		}

		public static QueryNoResult CreateUpdate<T>(this Schema.Schema schema, params T[] entities)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateUpdate(entities);
		}
	}
}
