using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Silk.Data.SQL.ORM.Queries
{
	public interface IQueryBuilder
	{
		void Source(QueryExpression source);
		void Offset(int? offset);
		void Limit(int? limit);

		void AndWhere(Condition condition);
		void OrWhere(Condition condition);

		void AndHaving(Condition condition);
		void OrHaving(Condition condition);

		SelectExpression CreateSelect();
	}

	public interface IQueryBuilder<T> : IQueryBuilder
		where T : class
	{
	}

	public interface IEntityQueryBuilder<TEntity> : IQueryBuilder<TEntity>
		where TEntity : class
	{
		IProjectionMapping<TView> Project<TView>()
			where TView : class;
	}
}
