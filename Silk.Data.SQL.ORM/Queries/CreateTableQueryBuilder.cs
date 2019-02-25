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
				GetAllLocalColumns().ToArray()
				);
		}

		private IEnumerable<ColumnDefinitionExpression> GetAllLocalColumns()
		{
			foreach (var field in _entityModel.Fields)
			{
				if (field.IsEntityLocalField)
				{
					if (field.Column != null)
					{
						yield return QueryExpression.DefineColumn(
							field.Column.Name, field.Column.DataType, field.Column.IsNullable,
							field.IsPrimaryKey && field.IsSeverGenerated, field.IsPrimaryKey
							);
					}
					foreach (var column in GetSubColumns(field))
						yield return column;
				}
			}

			IEnumerable<ColumnDefinitionExpression> GetSubColumns(EntityField field)
			{
				foreach (var subField in field.SubFields)
				{
					if (!subField.IsEntityLocalField)
						continue;

					if (subField.Column != null)
					{
						yield return QueryExpression.DefineColumn(
							subField.Column.Name, subField.Column.DataType, subField.Column.IsNullable,
							subField.IsPrimaryKey && subField.IsSeverGenerated, subField.IsPrimaryKey
							);
					}
					foreach (var column in GetSubColumns(subField))
						yield return column;
				}
			}
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
