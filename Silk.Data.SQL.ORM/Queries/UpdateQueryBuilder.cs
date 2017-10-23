using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class UpdateQueryBuilder<TSource>
		where TSource : new()
	{
		public DataModel<TSource> DataModel { get; }

		public UpdateQueryBuilder(DataModel<TSource> dataModel)
		{
			DataModel = dataModel;
		}

		public ICollection<QueryWithDelegate> CreateQuery(params TSource[] sources)
		{
			if (DataModel.PrimaryKeyFields == null ||
				DataModel.PrimaryKeyFields.Length == 0)
				throw new InvalidOperationException("A primary key is required.");

			//  todo: update this to work with datamodels that span multiple tables
			var table = DataModel.Fields.First().Storage.Table;
			var tableExpression = QueryExpression.Table(table.TableName);
			var queries = new List<QueryWithDelegate>();
			var columns = DataModel.Fields.Where(
				dataField => !dataField.Storage.IsPrimaryKey
				).ToArray();

			var modelReaders = sources.Select(source => new ObjectReadWriter(typeof(TSource), DataModel.Model, source)).ToArray();
			var viewContainers = sources.Select(source => new UpdateContainer(DataModel.Model, DataModel)).ToArray();

			DataModel.MapToView(modelReaders, viewContainers);

			foreach (var view in viewContainers)
			{
				queries.Add(new QueryWithDelegate(QueryExpression.Update(
					tableExpression,
					BuildPrimaryKeyWhereClause(view),
					columns.Select(q =>
					{
						if (view.AssignExpressions.TryGetValue(q.Name, out var val))
							return val;
						return QueryExpression.Assign(QueryExpression.Column(q.Storage.ColumnName), QueryExpression.Value(null));
					}).ToArray()
					)));
			}

			return queries;
		}

		private QueryExpression BuildPrimaryKeyWhereClause(IContainer container)
		{
			if (DataModel.PrimaryKeyFields == null ||
				DataModel.PrimaryKeyFields.Length == 0)
				throw new InvalidOperationException("A primary key is required.");

			QueryExpression where = null;
			foreach (var field in DataModel.PrimaryKeyFields)
			{
				var comparison = QueryExpression.Compare(
						QueryExpression.Column(field.Storage.ColumnName),
						ComparisonOperator.AreEqual,
						QueryExpression.Value(
							container.GetValue(field.ModelBinding.ViewFieldPath)
							));
				if (where == null)
				{
					where = comparison;
				}
				else
				{
					where = QueryExpression.AndAlso(where, comparison);
				}
			}
			return where;
		}
	}
}
