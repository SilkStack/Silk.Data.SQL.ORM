﻿using Silk.Data.SQL.Expressions;
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
					.SelectMany(field => field.Columns.Select(column => new { field, column }))
					.Select(q =>
						QueryExpression.DefineColumn(
							q.column.Name, q.column.DataType, q.column.IsNullable,
							q.field.IsPrimaryKey && q.field.IsSeverGenerated, q.field.IsPrimaryKey
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
					index.Fields.SelectMany(q => q.Columns).Select(q => q.Name).ToArray()
					);
			}
		}
	}
}
