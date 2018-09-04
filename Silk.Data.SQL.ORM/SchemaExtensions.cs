using Silk.Data.Modelling;
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
		{
			if (entities == null || entities.Length < 1)
				throw new Exception("At least 1 entity must be provided.");

			if (primaryKeyRequired && !schema.EntityFields.Any(q => q.IsPrimaryKey))
				throw new Exception("Entity type must have a primary key to generate update statements.");
		}

		public static QueryWithResult<int> CreateCount<T>(this EntitySchema<T> schema, Action<SelectBuilder<T>> queryCallback = null)
			where T : class
		{
			var queryBuilder = new SelectBuilder<T>(schema);
			var mapping = queryBuilder.Project(q => DatabaseFunctions.Count(q));
			queryCallback?.Invoke(queryBuilder);
			return new QueryWithScalarResult<int>(queryBuilder.BuildQuery(), mapping);
		}

		public static QueryWithResult<int> CreateCount<T>(this Schema.Schema schema, Action<SelectBuilder<T>> queryCallback = null)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateCount(queryCallback);
		}

		public static QueryWithResult<T> CreateSelect<T>(this EntitySchema<T> schema, Action<SelectBuilder<T>> queryCallback = null)
			where T : class
		{
			var queryBuilder = new SelectBuilder<T>(schema);
			var mapping = queryBuilder.Project<T>();
			queryCallback?.Invoke(queryBuilder);
			return new QueryWithMappedResult<T>(queryBuilder.BuildQuery(), mapping);
		}

		public static QueryWithResult<T> CreateSelect<T>(this Schema.Schema schema, Action<SelectBuilder<T>> queryCallback = null)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateSelect(queryCallback);
		}

		public static Query CreateInsert<T>(this EntitySchema<T> schema, params T[] entities)
			where T : class
		{
			SanityCheckArgs(schema, entities, primaryKeyRequired: false);

			var entityTypeModel = TypeModel.GetModelOf<T>();
			var serverGeneratedPrimaryKeyField = schema.EntityFields.FirstOrDefault(q => q.PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated);
			var isBulkInsert = serverGeneratedPrimaryKeyField == null;

			if (isBulkInsert)
				return new QueryNoResult(
					new CompositeQueryExpression(BuildExpressions())
					);

			return new QueryInjectResult<T> (
				new CompositeQueryExpression(BuildExpressions()),
				new ObjectResultMapper<T>(entities.Length, schema.EntityFields.Where(q => q.PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated).Select(q => q.GetValueBinding())),
				entities
				);

			IEnumerable<QueryExpression> BuildExpressions()
			{
				var queryBuilder = default(InsertBuilder<T>);
				if (isBulkInsert)
					queryBuilder = new InsertBuilder<T>(schema);

				foreach (var entity in entities)
				{
					if (!isBulkInsert)
						queryBuilder = new InsertBuilder<T>(schema);
					else
						queryBuilder.NewRow();

					foreach (var field in schema.EntityFields)
					{
						if (field.PrimaryKeyGenerator == PrimaryKeyGenerator.ServerGenerated)
							continue;

						if (field.PrimaryKeyGenerator == PrimaryKeyGenerator.ClientGenerated)
						{
							var newId = Guid.NewGuid();
							//  write generated ID to the object directly so that following queries can reference it
							var readWriter = new ObjectReadWriter(entity, entityTypeModel, typeof(T));
							readWriter.WriteField<Guid>(field.ModelPath, 0, newId);
						}

						queryBuilder.Set(field.GetFieldValuePair(entity));
					}

					if (!isBulkInsert)
					{
						yield return queryBuilder.BuildQuery();
						//  todo: when the SelectBuilder API is refactored come back here and make it sensible!
						var selectPKQueryBuilder = new SelectBuilder<T>(schema.Schema);
						selectPKQueryBuilder.Project<int>(QueryExpression.Alias(
							QueryExpression.LastInsertIdFunction(), "__PK_IDENTITY"));
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

		public static Query CreateDelete<T>(this EntitySchema<T> schema, params T[] entities)
			where T : class
		{
			SanityCheckArgs(schema, entities);

			return new QueryNoResult(
				new CompositeQueryExpression(BuildExpressions())
				);

			IEnumerable<QueryExpression> BuildExpressions()
			{
				foreach (var entity in entities)
				{
					var queryBuilder = new DeleteBuilder<T>(schema);
					foreach (var field in schema.EntityFields)
					{
						if (field.IsPrimaryKey)
							queryBuilder.AndWhere(field.GetFieldValuePair(entity), ComparisonOperator.AreEqual);
					}
					yield return queryBuilder.BuildQuery();
				}
			}
		}

		public static Query CreateDelete<T>(this Schema.Schema schema, params T[] entities)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateDelete(entities);
		}

		public static Query CreateUpdate<T>(this EntitySchema<T> schema, params T[] entities)
			where T : class
		{
			SanityCheckArgs(schema, entities);

			return new QueryNoResult(
				new CompositeQueryExpression(BuildExpressions())
				);

			IEnumerable<QueryExpression> BuildExpressions()
			{
				foreach (var entity in entities)
				{
					var queryBuilder = new UpdateBuilder<T>(schema);
					foreach (var field in schema.EntityFields)
					{
						if (field.IsPrimaryKey)
							queryBuilder.AndWhere(field.GetFieldValuePair(entity), ComparisonOperator.AreEqual);
						else
							queryBuilder.Set(field.GetFieldValuePair(entity));
					}
					yield return queryBuilder.BuildQuery();
				}
			}
		}

		public static Query CreateUpdate<T>(this Schema.Schema schema, params T[] entities)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateUpdate(entities);
		}
	}
}
