﻿using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class CreateTableQueryBuilder<T> : IEntityQueryBuilder
		where T : class
	{
		private readonly EntityModel<T> _entityModel;

		public CreateTableQueryBuilder(EntityModel<T> entityModel)
		{
			_entityModel = entityModel;
		}

		public QueryExpression BuildQuery()
			=> BuildQuery(_entityModel.TableName);

		public QueryExpression BuildQuery(string tableName)
		{
			var query = new CompositeQueryExpression();
			query.Queries.Add(GetCreateTableExpression(tableName));
			return query;
		}

		private CreateTableExpression GetCreateTableExpression(string tableName)
		{
			return QueryExpression.CreateTable(
				tableName,
				_entityModel.Fields
					.SelectMany(field => field.Columns.Select(column => new { field, column }))
					.Select(q =>
						QueryExpression.DefineColumn(
							q.column.Name, q.column.DataType, q.column.IsNullable,
							q.field.IsPrimaryKey && q.field.IsSeverGenerated, q.field.IsPrimaryKey
							)
				));
		}
	}
}
