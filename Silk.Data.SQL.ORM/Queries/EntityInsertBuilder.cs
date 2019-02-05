using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	/// <summary>
	/// An INSERT statement builder for entities of type T.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EntityInsertBuilder<T> : QueryBuilderBase<T>, IEntityInsertQueryBuilder<T>
		where T : class
	{
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
		IFieldAssignmentBuilder IFieldAssignmentQueryBuilder.Assignments { get => Assignments; set => Assignments = (IEntityFieldAssignmentBuilder<T>)value; }

		public EntityInsertBuilder(Schema.Schema schema, ObjectReadWriter entityReadWriter = null)
			: base(schema, entityReadWriter)
		{
		}

		public EntityInsertBuilder(EntitySchema<T> schema, ObjectReadWriter entityReadWriter = null)
			: base(schema, entityReadWriter)
		{
		}

		public override QueryExpression BuildQuery()
		{
			var row = _assignments?.Build();
			var columnNames = row?.Select(q => q.Column.ColumnName).ToArray() ?? new string[0];
			var values = row?.Select(q => q.Expression).ToArray() ?? new QueryExpression[0];

			return QueryExpression.Insert(
				Source.TableName,
				columnNames,
				values
				);
		}
	}
}
