﻿using Silk.Data.Modelling;
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

		public static QueryNoResult CreateInsert<T>(this EntitySchema<T> schema, Action<InsertBuilder<T>> queryCallback)
			where T : class
		{
			var builder = new InsertBuilder<T>(schema);
			queryCallback?.Invoke(builder);
			return new QueryNoResult(builder.BuildQuery());
		}

		public static QueryNoResult CreateInsert<T>(this Schema.Schema schema, Action<InsertBuilder<T>> queryCallback)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
			return entitySchema.CreateInsert<T>(queryCallback);
		}

		public static QueryNoResult CreateUpdate<T>(this EntitySchema<T> schema, Action<UpdateBuilder<T>> queryCallback)
			where T : class
		{
			var builder = new UpdateBuilder<T>(schema);
			queryCallback?.Invoke(builder);
			return new QueryNoResult(builder.BuildQuery());
		}

		public static QueryNoResult CreateUpdate<T>(this Schema.Schema schema, Action<UpdateBuilder<T>> queryCallback)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
			return entitySchema.CreateUpdate<T>(queryCallback);
		}

		public static QueryNoResult CreateDelete<T>(this EntitySchema<T> schema, Action<DeleteBuilder<T>> queryCallback)
			where T : class
		{
			var builder = new DeleteBuilder<T>(schema);
			queryCallback?.Invoke(builder);
			return new QueryNoResult(builder.BuildQuery());
		}

		public static QueryNoResult CreateDelete<T>(this Schema.Schema schema, Action<DeleteBuilder<T>> queryCallback)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");
			return entitySchema.CreateDelete<T>(queryCallback);
		}

		public static QueryWithScalarResult<bool> CreateTableExists<TLeft, TRight>(this Relationship<TLeft, TRight> relationship)
			where TLeft : class
			where TRight : class
		{
			return new QueryWithScalarResult<bool>(
				QueryExpression.TableExists(relationship.JunctionTable.TableName),
				new ValueResultMapper<bool>(1, null)
				);
		}

		public static QueryWithScalarResult<bool> CreateTableExists<TLeft, TRight>(this Schema.Schema schema, string relationshipName)
			where TLeft : class
			where TRight : class
		{
			var relationship = schema.GetRelationship<TLeft, TRight>(relationshipName);
			if (relationship == null)
				throw new Exception("Relationship isn't configured in schema.");

			return relationship.CreateTableExists();
		}

		public static QueryWithScalarResult<bool> CreateTableExists<T>(this EntitySchema<T> schema)
			where T : class
		{
			return new QueryWithScalarResult<bool>(
				QueryExpression.TableExists(schema.EntityTable.TableName),
				new ValueResultMapper<bool>(1, null)
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

		public static QueryNoResult CreateTable<TLeft, TRight>(this Relationship<TLeft, TRight> relationship)
			where TLeft : class
			where TRight : class
		{
			var queryBuilder = new CreateTableBuilder<TLeft, TRight>(relationship);
			return new QueryNoResult(queryBuilder.BuildQuery());
		}

		public static QueryNoResult CreateTable<TLeft, TRight>(this Schema.Schema schema, string relationshipName)
			where TLeft : class
			where TRight : class
		{
			var relationship = schema.GetRelationship<TLeft, TRight>(relationshipName);
			if (relationship == null)
				throw new Exception("Relationship isn't configured in schema.");

			return relationship.CreateTable();
		}

		public static QueryNoResult CreateTable<T>(this EntitySchema<T> schema)
			where T : class
		{
			var queryBuilder = new CreateTableBuilder<T>(schema);
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

		public static QueryWithScalarResult<int> CreateCount<T>(this EntitySchema<T> schema, Action<SelectBuilder<T>> queryCallback = null)
			where T : class
		{
			var queryBuilder = new SelectBuilder<T>(schema);
			var mapping = queryBuilder.Project(q => DatabaseFunctions.Count(q));
			queryCallback?.Invoke(queryBuilder);
			return new QueryWithScalarResult<int>(queryBuilder.BuildQuery(), mapping);
		}

		public static QueryWithScalarResult<int> CreateCount<T>(this Schema.Schema schema, Action<SelectBuilder<T>> queryCallback = null)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateCount(queryCallback);
		}

		public static QueryWithScalarResult<int> CreateCount<TLeft, TRight>(this Relationship<TLeft, TRight> relationship, Action<SelectBuilder<TLeft, TRight>> queryCallback = null)
			where TLeft : class
			where TRight : class
		{
			var queryBuilder = new SelectBuilder<TLeft, TRight>(relationship);
			var mapping = queryBuilder.Project((left, right) => DatabaseFunctions.Count(left));
			queryCallback?.Invoke(queryBuilder);
			return new QueryWithScalarResult<int>(queryBuilder.BuildQuery(), mapping);
		}

		public static QueryWithScalarResult<int> CreateCount<TLeft, TRight>(this Schema.Schema schema, string relationshipName, Action<SelectBuilder<TLeft, TRight>> queryCallback = null)
			where TLeft : class
			where TRight : class
		{
			var relationship = schema.GetRelationship<TLeft, TRight>(relationshipName);
			if (relationship == null)
				throw new Exception("Relationship isn't configured in schema.");

			return relationship.CreateCount(queryCallback);
		}

		public static QueryWithTupleResult<TLeft, TRight> CreateSelect<TLeft, TRight>(this Relationship<TLeft, TRight> relationship, Action<SelectBuilder<TLeft, TRight>> queryCallback = null)
			where TLeft : class
			where TRight : class
		{
			var queryBuilder = new SelectBuilder<TLeft, TRight>(relationship);
			var mapping = queryBuilder.Project<TLeft, TRight>();
			queryCallback?.Invoke(queryBuilder);
			return new QueryWithTupleResult<TLeft, TRight>(queryBuilder.BuildQuery(), mapping);
		}

		public static QueryWithTupleResult<TLeft, TRight> CreateSelect<TLeft, TRight>(this Schema.Schema schema, string relationshipName, Action<SelectBuilder<TLeft, TRight>> queryCallback = null)
			where TLeft : class
			where TRight : class
		{
			var relationship = schema.GetRelationship<TLeft, TRight>(relationshipName);
			if (relationship == null)
				throw new Exception("Relationship isn't configured in schema.");

			return relationship.CreateSelect(queryCallback);
		}

		public static QueryWithTupleResult<TLeftView, TRightView> CreateSelect<TLeft, TRight, TLeftView, TRightView>(this Relationship<TLeft, TRight> relationship, Action<SelectBuilder<TLeft, TRight>> queryCallback = null)
			where TLeft : class
			where TRight : class
			where TLeftView : class
			where TRightView : class
		{
			var queryBuilder = new SelectBuilder<TLeft, TRight>(relationship);
			var mapping = queryBuilder.Project<TLeftView, TRightView>();
			queryCallback?.Invoke(queryBuilder);
			return new QueryWithTupleResult<TLeftView, TRightView>(queryBuilder.BuildQuery(), mapping);
		}

		public static QueryWithTupleResult<TLeftView, TRightView> CreateSelect<TLeft, TRight, TLeftView, TRightView>(this Schema.Schema schema, string relationshipName, Action<SelectBuilder<TLeft, TRight>> queryCallback = null)
			where TLeft : class
			where TRight : class
			where TLeftView : class
			where TRightView : class
		{
			var relationship = schema.GetRelationship<TLeft, TRight>(relationshipName);
			if (relationship == null)
				throw new Exception("Relationship isn't configured in schema.");

			return relationship.CreateSelect<TLeft, TRight, TLeftView, TRightView>(queryCallback);
		}

		public static QueryWithMappedResult<T> CreateSelect<T>(this EntitySchema<T> schema, Action<SelectBuilder<T>> queryCallback = null)
			where T : class
		{
			var queryBuilder = new SelectBuilder<T>(schema);
			var mapping = queryBuilder.Project<T>();
			queryCallback?.Invoke(queryBuilder);
			return new QueryWithMappedResult<T>(queryBuilder.BuildQuery(), mapping);
		}

		public static QueryWithMappedResult<T> CreateSelect<T>(this Schema.Schema schema, Action<SelectBuilder<T>> queryCallback = null)
			where T : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateSelect(queryCallback);
		}

		public static QueryWithMappedResult<TView> CreateSelect<T, TView>(this EntitySchema<T> schema, Action<SelectBuilder<T>> queryCallback = null)
			where T : class
			where TView : class
		{
			var queryBuilder = new SelectBuilder<T>(schema);
			var mapping = queryBuilder.Project<TView>();
			queryCallback?.Invoke(queryBuilder);
			return new QueryWithMappedResult<TView>(queryBuilder.BuildQuery(), mapping);
		}

		public static QueryWithMappedResult<TView> CreateSelect<T, TView>(this Schema.Schema schema, Action<SelectBuilder<T>> queryCallback = null)
			where T : class
			where TView : class
		{
			var entitySchema = schema.GetEntitySchema<T>();
			if (entitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			return entitySchema.CreateSelect<T, TView>(queryCallback);
		}

		public static QueryNoResult CreateInsert<TLeft, TRight>(this Relationship<TLeft, TRight> relationship, TLeft left, params TRight[] right)
			where TLeft : class
			where TRight : class
		{
			if (left == null)
				throw new ArgumentNullException(nameof(left));
			if (right == null || right.Length == 0)
				throw new ArgumentException("At least one related entity must be provided.", nameof(right));

			var queryBuilder = new InsertBuilder<TLeft, TRight>(relationship);
			foreach (var entity in right)
			{
				queryBuilder.NewRow();

				queryBuilder.Set(relationship.LeftRelationship.GetFieldValuePair(left));
				queryBuilder.Set(relationship.RightRelationship.GetFieldValuePair(entity));
			}

			return new QueryNoResult(queryBuilder.BuildQuery());
		}

		public static QueryNoResult CreateInsert<TLeft, TRight>(this Schema.Schema schema, string relationshipName, TLeft left, params TRight[] right)
			where TLeft : class
			where TRight : class
		{
			var relationship = schema.GetRelationship<TLeft, TRight>(relationshipName);
			if (relationship == null)
				throw new Exception("Relationship isn't configured in schema.");

			return relationship.CreateInsert(left, right);
		}

		public static QueryNoResult CreateInsert<TLeft, TRight>(this Relationship<TLeft, TRight> relationship, TRight right, params TLeft[] left)
			where TLeft : class
			where TRight : class
		{
			if (right == null)
				throw new ArgumentNullException(nameof(right));
			if (left == null || left.Length == 0)
				throw new ArgumentException("At least one related entity must be provided.", nameof(left));

			var queryBuilder = new InsertBuilder<TLeft, TRight>(relationship);
			foreach (var entity in left)
			{
				queryBuilder.NewRow();

				queryBuilder.Set(relationship.LeftRelationship.GetFieldValuePair(entity));
				queryBuilder.Set(relationship.RightRelationship.GetFieldValuePair(right));
			}

			return new QueryNoResult(queryBuilder.BuildQuery());
		}

		public static QueryNoResult CreateInsert<TLeft, TRight>(this Schema.Schema schema, string relationshipName, TRight right, params TLeft[] left)
			where TLeft : class
			where TRight : class
		{
			var relationship = schema.GetRelationship<TLeft, TRight>(relationshipName);
			if (relationship == null)
				throw new Exception("Relationship isn't configured in schema.");

			return relationship.CreateInsert(right, left);
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

		public static QueryNoResult CreateDelete<TLeft, TRight>(this Relationship<TLeft, TRight> relationship, TLeft left, params TRight[] right)
			where TLeft : class
			where TRight : class
		{
			if (left == null)
				throw new ArgumentNullException(nameof(left));

			var queryBuilder = new DeleteBuilder<TLeft, TRight>(relationship);
			queryBuilder.AndWhere(relationship.LeftRelationship.GetFieldValuePair(left), ComparisonOperator.AreEqual);
			if (right != null && right.Length > 0)
			{
				var rightExpression = default(QueryExpression);
				foreach (var entity in right)
				{
					var entityExpression = default(QueryExpression);
					foreach (var (columnExpression, valueExpression) in relationship.RightRelationship.GetFieldValuePair(entity).GetColumnExpressionPairs())
					{
						entityExpression = QueryExpression.CombineConditions(
							entityExpression,
							ConditionType.AndAlso,
							QueryExpression.Compare(
								columnExpression,
								ComparisonOperator.AreEqual,
								valueExpression
							));
					}
					rightExpression = QueryExpression.CombineConditions(rightExpression, ConditionType.OrElse, entityExpression);
				}
				queryBuilder.AndWhere(rightExpression);
			}

			return new QueryNoResult(queryBuilder.BuildQuery());
		}

		public static QueryNoResult CreateDelete<TLeft, TRight>(this Schema.Schema schema, string relationshipName, TLeft left, params TRight[] right)
			where TLeft : class
			where TRight : class
		{
			var relationship = schema.GetRelationship<TLeft, TRight>(relationshipName);
			if (relationship == null)
				throw new Exception("Relationship isn't configured in schema.");

			return relationship.CreateDelete(left, right);
		}

		public static QueryNoResult CreateDelete<TLeft, TRight>(this Relationship<TLeft, TRight> relationship, TRight right, params TLeft[] left)
			where TLeft : class
			where TRight : class
		{
			if (right == null)
				throw new ArgumentNullException(nameof(right));

			var queryBuilder = new DeleteBuilder<TLeft, TRight>(relationship);
			queryBuilder.AndWhere(relationship.RightRelationship.GetFieldValuePair(right), ComparisonOperator.AreEqual);
			if (left != null && left.Length > 0)
			{
				var rightExpression = default(QueryExpression);
				foreach (var entity in left)
				{
					var entityExpression = default(QueryExpression);
					foreach (var (columnExpression, valueExpression) in relationship.LeftRelationship.GetFieldValuePair(entity).GetColumnExpressionPairs())
					{
						entityExpression = QueryExpression.CombineConditions(
							entityExpression,
							ConditionType.AndAlso,
							QueryExpression.Compare(
								columnExpression,
								ComparisonOperator.AreEqual,
								valueExpression
							));
					}
					rightExpression = QueryExpression.CombineConditions(rightExpression, ConditionType.OrElse, entityExpression);
				}
				queryBuilder.AndWhere(rightExpression);
			}

			return new QueryNoResult(queryBuilder.BuildQuery());
		}

		public static QueryNoResult CreateDelete<TLeft, TRight>(this Schema.Schema schema, string relationshipName, TRight right, params TLeft[] left)
			where TLeft : class
			where TRight : class
		{
			var relationship = schema.GetRelationship<TLeft, TRight>(relationshipName);
			if (relationship == null)
				throw new Exception("Relationship isn't configured in schema.");

			return relationship.CreateDelete(right, left);
		}

		public static QueryNoResult CreateDelete<T>(this EntitySchema<T> schema, params T[] entities)
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