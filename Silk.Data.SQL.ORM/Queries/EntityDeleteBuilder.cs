using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;

namespace Silk.Data.SQL.ORM.Queries
{
	public class EntityDeleteBuilder<T> : QueryBuilderBase<T>, IEntityDeleteQueryBuilder<T>
		where T : class
	{
		private IEntityConditionBuilder<T> _where;
		public IEntityConditionBuilder<T> Where
		{
			get
			{
				if (_where == null)
					_where = new DefaultEntityConditionBuilder<T>(EntitySchema, ExpressionConverter);
				return _where;
			}
			set => _where = value;
		}

		IConditionBuilder IWhereQueryBuilder.Where { get => Where; set => Where = (IEntityConditionBuilder<T>)value; }

		public EntityDeleteBuilder(Schema.Schema schema, ObjectReadWriter entityReadWriter = null) : base(schema, entityReadWriter) { }

		public EntityDeleteBuilder(EntitySchema<T> schema, ObjectReadWriter entityReadWriter = null) : base(schema, entityReadWriter) { }

		public override QueryExpression BuildQuery()
		{
			var where = _where?.Build();

			if (where?.RequiredJoins != null && where?.RequiredJoins.Length > 0)
				throw new InvalidOperationException("Query requires one or more JOINs, use sub-queries instead.");

			return QueryExpression.Delete(
				Source, where?.QueryExpression
				);
		}
	}
}
