using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface IFieldAssignmentBuilder
	{
		AssignColumnExpression[] Build();

		void Set(ColumnExpression columnExpression, QueryExpression valueExpression);
		void Set(ColumnExpression columnExpression, object value);
	}

	public interface IEntityFieldAssignmentBuilder<T> : IFieldAssignmentBuilder
		where T : class
	{
		void Set(EntityField<T> schemaField, T entity);
		void Set<TValue>(EntityField<T> schemaField, TValue value);
		void Set<TValue>(EntityField<T> schemaField, Expression<Func<T, TValue>> valueExpression);
		void Set(EntityField<T> schemaField, IQueryBuilder subQuery);
		void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, TProperty value);
		void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, Expression<Func<T, TProperty>> valueExpression);
		void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, Expression valueExpression);
		void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, IQueryBuilder subQuery);
		void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, QueryExpression valueExpression);
	}
}
