using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class CreateTableQueryBuilder<T> : IEntityQueryBuilder<T>
		where T : class
	{
		private readonly EntityModel<T> _entityModel;

		public CreateTableQueryBuilder(EntityModel<T> entityModel)
		{
			_entityModel = entityModel;
		}

		public QueryExpression BuildQuery()
		{
			var query = new CompositeQueryExpression();
			query.Queries.Add(GetCreateTableExpression());
			query.Queries.AddRange(GetCreateIndexExpressions());
			return query;
		}

		private CreateTableExpression GetCreateTableExpression()
		{
			return QueryExpression.CreateTable(
				_entityModel.Table.TableName,
				_entityModel.Fields
					.Where(field => field.Column != null)
					.Select(q =>
						QueryExpression.DefineColumn(
							q.Column.Name, q.Column.DataType, q.Column.IsNullable,
							q.IsPrimaryKey && q.IsSeverGenerated, q.IsPrimaryKey
							)
				));
		}

		private IEnumerable<QueryExpression> GetCreateIndexExpressions()
		{
			foreach (var index in _entityModel.Indexes)
			{
				yield return QueryExpression.CreateIndex(
					_entityModel.Table.TableName,
					index.HasUniqueConstraint,
					index.Fields.Where(q => q.Column != null).Select(q => q.Column.Name).ToArray()
					);
			}
		}
	}
}
