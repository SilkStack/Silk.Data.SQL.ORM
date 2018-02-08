using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.NewModelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DeleteQueryBuilder<TSource>
		where TSource : new()
	{
		public EntitySchema<TSource> EntitySchema { get; }

		public DeleteQueryBuilder(EntitySchema<TSource> entitySchema)
		{
			EntitySchema = entitySchema;
		}

		public ICollection<ORMQuery> CreateQuery(params TSource[] sources)
		{
			return CreateQuery(EntitySchema, sources);
		}

		public ICollection<ORMQuery> CreateQuery(IEnumerable<TSource> sources)
		{
			return CreateQuery(EntitySchema, sources);
		}

		public ICollection<ORMQuery> CreateQuery<TView>(params TView[] sources)
			where TView : new()
		{
			return CreateQuery(EntitySchema.GetProjection<TView>(), sources);
		}

		public ICollection<ORMQuery> CreateQuery<TView>(IEnumerable<TView> sources)
			where TView : new()
		{
			return CreateQuery(EntitySchema.GetProjection<TView>(), sources);
		}

		public ICollection<ORMQuery> CreateQuery<TView>(IEntitySchema<TView> schema, IEnumerable<TView> sources)
			where TView : new()
		{
			if (schema.EntityTable.PrimaryKeyFields.Length == 0)
				throw new InvalidOperationException("Entities require a primary key to be deleted.");

			var primaryKeyFields = GetPrimaryKeyFields(schema);

			var sourceReadWriters = sources
				.Select(q => new ObjectModelReadWriter(schema.Model, q))
				.ToArray();

			QueryExpression whereExpr = null;
			foreach (var source in sourceReadWriters)
			{
				QueryExpression sourceWhere = null;
				foreach (var field in primaryKeyFields)
				{
					var pkCondition = QueryExpression.Compare(
						QueryExpression.Column(field.Name),
						ComparisonOperator.AreEqual,
						QueryExpression.Value(field.ModelBinding.ReadValue<object>(source))
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

			return new ORMQuery[] {
				new NoResultORMQuery(
					QueryExpression.Delete(
						QueryExpression.Table(schema.EntityTable.TableName),
						whereConditions: whereExpr
					)
				)
			};
		}

		private IDataField[] GetPrimaryKeyFields(IEntitySchema schema)
		{
			var tablePrimaryKeyFields = schema.EntityTable.PrimaryKeyFields;
			var ret = new IDataField[tablePrimaryKeyFields.Length];
			for (var i = 0; i < ret.Length; i++)
			{
				var foundField = schema.Fields.FirstOrDefault(q => q.ModelBinding.ViewFieldPath.SequenceEqual(
					tablePrimaryKeyFields[i].ModelBinding.ViewFieldPath
					));
				if (foundField == null)
					throw new InvalidOperationException("A primary key is missing from the projection.");
				ret[i] = foundField;
			}
			return ret;
		}

		private ICollection<ORMQuery> CreateQuery<TView>(EntityModel<TView> model, IEnumerable<TView> sources)
			where TView : new()
		{
			if (model.PrimaryKeyFields == null ||
				model.PrimaryKeyFields.Length == 0)
				throw new InvalidOperationException("A primary key is required.");

			var sourceReadWriters = sources
				.Select(q => new ObjectModelReadWriter(model.Model, q))
				.ToArray();

			var queries = new List<ORMQuery>();

			QueryExpression whereExpr = null;
			foreach (var sourceReadWriter in sourceReadWriters)
			{
				QueryExpression sourceWhere = null;
				foreach (var primaryKey in model.PrimaryKeyFields)
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

			var schema = model.Schema;
			var manyToManyFields = model.Fields
				.Where(q => q.Storage == null && q.Relationship != null && q.Relationship.RelationshipType == RelationshipType.ManyToMany)
				.ToArray();
			if (manyToManyFields.Length > 0)
			{
				foreach (var sourceReadWriter in sourceReadWriters)
				{
					foreach (var field in manyToManyFields)
					{
						var joinTable = schema.Tables.FirstOrDefault(q => q.IsJoinTableFor(model.Schema.EntityTable.EntityType, field.Relationship.ForeignModel.EntityType));
						if (joinTable == null)
							throw new InvalidOperationException($"Couldn't locate join table for '{field.Relationship.ForeignModel.EntityType.FullName}'.");

						QueryExpression deleteWhereExpr = null;
						foreach (var joinTableField in joinTable.DataFields.Where(
							q => q.RelatedEntityType == schema.EntityTable.EntityType
							))
						{
							var pkCondition = QueryExpression.Compare(
								QueryExpression.Column(joinTableField.Storage.ColumnName),
								ComparisonOperator.AreEqual,
								QueryExpression.Value(joinTableField.ModelBinding.ReadValue<object>(sourceReadWriter))
							);

							if (deleteWhereExpr == null)
								deleteWhereExpr = pkCondition;
							else
								deleteWhereExpr = QueryExpression.AndAlso(deleteWhereExpr, pkCondition);
						}

						if (deleteWhereExpr == null)
							throw new InvalidOperationException("Could not determine DELETE condition for many to many relationship.");

						queries.Add(new NoResultORMQuery(QueryExpression.Delete(
							QueryExpression.Table(joinTable.TableName),
							deleteWhereExpr
							)));
					}
				}
			}

			queries.Add(new NoResultORMQuery(
				QueryExpression.Delete(
					QueryExpression.Table(model.Schema.EntityTable.TableName),
					whereConditions: whereExpr
				)));

			return queries;
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
						QueryExpression.Table(EntitySchema.EntityTable.TableName),
						whereConditions: where
					))
			};
		}
	}
}
