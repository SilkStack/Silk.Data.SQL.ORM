using System;
using System.Linq;
using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Queries
{
	public class EntityUpdateBuilder<T> : QueryBuilderBase<T>, IUpdateQueryBuilder<T>
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

		private IEntityFieldAssignmentBuilder<T> _assignments;
		public IEntityFieldAssignmentBuilder<T> Assignments
		{
			get
			{
				if (_assignments == null)
					_assignments = new DefaultEntityFieldAssignmentBuilder<T>(EntitySchema, ExpressionConverter);
				return _assignments;
			}
			set => _assignments = value;
		}

		IConditionBuilder IWhereQueryBuilder.Where { get => Where; set => Where = (IEntityConditionBuilder<T>)value; }
		IFieldAssignmentBuilder IFieldAssignmentQueryBuilder.Assignments { get => Assignments; set => Assignments = (IEntityFieldAssignmentBuilder<T>)value; }

		public EntityUpdateBuilder(Schema.Schema schema, ObjectReadWriter entityReadWriter = null) : base(schema, entityReadWriter) { }

		public EntityUpdateBuilder(EntitySchema<T> schema, ObjectReadWriter entityReadWriter = null) : base(schema, entityReadWriter) { }

		public override QueryExpression BuildQuery()
		{
			var where = _where?.Build();
			var row = _assignments?.Build();

			if (where?.RequiredJoins != null && where?.RequiredJoins.Length > 0)
				throw new InvalidOperationException("Query requires one or more JOINs, use sub-queries instead.");

			return QueryExpression.Update(
				Source,
				where: where?.QueryExpression,
				assignments: row
				);
		}
	}
}
