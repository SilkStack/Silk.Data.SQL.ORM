using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class DeleteQueryBuilder<TSource>
		where TSource : new()
	{
		public EntityModel<TSource> DataModel { get; }

		public DeleteQueryBuilder(EntityModel<TSource> dataModel)
		{
			DataModel = dataModel;
		}

		public ICollection<ORMQuery> CreateQuery(params TSource[] sources)
		{
			if (sources == null || sources.Length < 1)
				throw new ArgumentException("At least one source must be provided.", nameof(sources));

			if (DataModel.PrimaryKeyFields == null ||
				DataModel.PrimaryKeyFields.Length == 0)
				throw new InvalidOperationException("A primary key is required.");

			var sourceReadWriters = sources
				.Select(q => new ObjectModelReadWriter(DataModel.Model, q))
				.ToArray();

			QueryExpression whereExpr = null;
			foreach (var sourceReadWriter in sourceReadWriters)
			{
				QueryExpression sourceWhere = null;
				foreach (var primaryKey in DataModel.PrimaryKeyFields)
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

			return new ORMQuery[]
			{
				new NoResultORMQuery(
					QueryExpression.Delete(
						QueryExpression.Table(DataModel.Name),
						whereConditions: whereExpr
					))
			};
		}

		//public ICollection<QueryWithDelegate> CreateQuery(params TSource[] sources)
		//{
		//	if (DataModel.PrimaryKeyFields == null ||
		//		DataModel.PrimaryKeyFields.Length == 0)
		//		throw new InvalidOperationException("A primary key is required.");

		//	//  todo: update this to work with datamodels that span multiple tables
		//	var table = DataModel.Schema.Tables.First(q => q.IsEntityTable);
		//	var tableExpression = QueryExpression.Table(table.TableName);
		//	var queries = new List<QueryWithDelegate>();
		//	var columns = DataModel.Fields.Where(
		//		dataField => !dataField.Storage.IsPrimaryKey
		//		).ToArray();

		//	var modelReaders = sources.Select(source => new ObjectReadWriter(typeof(TSource), DataModel.Model, source)).ToArray();
		//	var viewContainers = sources.Select(source => new UpdateContainer(DataModel.Model, DataModel)).ToArray();

		//	//  todo: consider reading primary key fields directly from TSource instead of mapping
		//	DataModel.MapToView(modelReaders, viewContainers);

		//	foreach (var view in viewContainers)
		//	{
		//		queries.Add(new QueryWithDelegate(QueryExpression.Delete(
		//			tableExpression,
		//			BuildPrimaryKeyWhereClause(view)
		//			)));
		//	}

		//	return queries;
		//}

		//public ICollection<QueryWithDelegate> CreateQuery(
		//	QueryExpression where = null
		//	)
		//{
		//	//  todo: update this to work with datamodels that span multiple tables
		//	var table = DataModel.Schema.Tables.First(q => q.IsEntityTable);
		//	var tableExpression = QueryExpression.Table(table.TableName);
		//	var queries = new List<QueryWithDelegate>
		//	{
		//		new QueryWithDelegate(QueryExpression.Delete(
		//			tableExpression,
		//			where
		//		))
		//	};

		//	return queries;
		//}

		//private QueryExpression BuildPrimaryKeyWhereClause(IContainer container)
		//{
		//	if (DataModel.PrimaryKeyFields == null ||
		//		DataModel.PrimaryKeyFields.Length == 0)
		//		throw new InvalidOperationException("A primary key is required.");

		//	QueryExpression where = null;
		//	foreach (var field in DataModel.PrimaryKeyFields)
		//	{
		//		var comparison = QueryExpression.Compare(
		//				QueryExpression.Column(field.Storage.ColumnName),
		//				ComparisonOperator.AreEqual,
		//				QueryExpression.Value(
		//					container.GetValue(field.ModelBinding.ViewFieldPath)
		//					));
		//		if (where == null)
		//		{
		//			where = comparison;
		//		}
		//		else
		//		{
		//			where = QueryExpression.AndAlso(where, comparison);
		//		}
		//	}
		//	return where;
		//}
	}
}
