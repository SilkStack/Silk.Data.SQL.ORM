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
	public class UpdateQueryBuilder<TSource>
		where TSource : new()
	{
		public EntityModel<TSource> DataModel { get; }

		public UpdateQueryBuilder(EntityModel<TSource> dataModel)
		{
			DataModel = dataModel;
		}

		public ICollection<ORMQuery> CreateQuery(params TSource[] sources)
		{
			return CreateQuery(DataModel, sources);
		}

		public ICollection<ORMQuery> CreateQuery<TView>(params TView[] sources)
			where TView : new()
		{
			return CreateQuery(DataModel.Domain.GetProjectionModel<TSource, TView>(), sources);
		}

		public ICollection<ORMQuery> CreateQuery<TView>(EntityModel<TView> model, params TView[] sources)
			where TView : new()
		{
			if (sources == null || sources.Length < 1)
				throw new ArgumentException("At least one source must be provided.", nameof(sources));

			if (model.PrimaryKeyFields == null ||
				model.PrimaryKeyFields.Length == 0)
				throw new InvalidOperationException("A primary key is required.");

			var sourceReadWriters = sources
				.Select(q => new ObjectModelReadWriter(model.Model, q))
				.ToArray();

			var queries = new List<ORMQuery>();
			foreach (var sourceReadWriter in sourceReadWriters)
			{
				var row = new Dictionary<DataField, QueryExpression>();
				foreach (var field in model.Fields)
				{
					if (field.Storage == null)
						continue;

					if (field.Relationship == null)
					{
						row.Add(field, QueryExpression.Value(
							field.ModelBinding.ReadValue<object>(sourceReadWriter)
							));
					}
					else
					{
						row.Add(field, new LateReadValueExpression(() =>
							field.ModelBinding.ReadValue<object>(sourceReadWriter)
							));
					}
				}

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

				queries.Add(new NoResultORMQuery(
					QueryExpression.Update(
						QueryExpression.Table(model.Schema.EntityTable.TableName),
						where: sourceWhere,
						assignments: row.Select(kvp => QueryExpression.Assign(
							QueryExpression.Column(kvp.Key.Storage.ColumnName),
							kvp.Value
							)).ToArray()
					)));
			}

			var schema = model.Schema;
			var manyToManyFields = model.Fields
				.Where(q => q.Storage == null && q.Relationship != null && q.Relationship.RelationshipType == RelationshipType.ManyToMany)
				.ToArray();
			if (manyToManyFields.Length > 0)
			{
				var rows = new List<QueryExpression[]>();
				foreach (var sourceReadWriter in sourceReadWriters)
				{
					foreach (var field in manyToManyFields)
					{
						rows.Clear();

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

						queries.Add(new NoResultORMQuery(QueryExpression.Delete(
							QueryExpression.Table(joinTable.TableName),
							deleteWhereExpr
							)));

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
