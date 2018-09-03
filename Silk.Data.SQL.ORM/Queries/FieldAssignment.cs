using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Queries
{
	public abstract class FieldAssignment
	{
		public IEntityField Field { get; }

		public FieldAssignment(IEntityField field)
		{
			Field = field;
		}

		public abstract IEnumerable<(Column Column, QueryExpression ValueExpression)> GetColumnExpressionPairs();
	}

	public class FieldValueAssignment<T> : FieldAssignment
	{
		public new IEntityFieldOfValue<T> Field { get; }
		public IValueReader<T> ValueReader { get; }

		public FieldValueAssignment(IEntityFieldOfValue<T> field, IValueReader<T> valueReader)
			: base(field)
		{
			Field = field;
			ValueReader = valueReader;
		}

		public override IEnumerable<(Column Column, QueryExpression ValueExpression)> GetColumnExpressionPairs()
		{
			foreach (var column in Field.Columns)
			{
				yield return (column, QueryExpression.Value(ValueReader.Read()));
			}
		}
	}
}
