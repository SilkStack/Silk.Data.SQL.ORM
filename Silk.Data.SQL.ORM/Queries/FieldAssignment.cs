using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public abstract class FieldAssignment
	{
		public abstract IEnumerable<(ColumnExpression ColumnExpression, QueryExpression ValueExpression)> GetColumnExpressionPairs();
	}

	public class FieldExpressionAssignment<T> : FieldAssignment
	{
		private readonly ColumnExpression _columnExpression;
		private readonly QueryExpression _valueExpression;

		public FieldExpressionAssignment(ColumnExpression columnExpression, QueryExpression valueExpression)
		{
			_columnExpression = columnExpression;
			_valueExpression = valueExpression;
		}

		public override IEnumerable<(ColumnExpression ColumnExpression, QueryExpression ValueExpression)> GetColumnExpressionPairs()
		{
			yield return (_columnExpression, _valueExpression);
		}
	}

	public class FieldValueAssignment<T> : FieldAssignment
	{
		public IEntityFieldOfValue<T> Field { get; }
		public IValueReader<T> ValueReader { get; }

		public FieldValueAssignment(IEntityFieldOfValue<T> field, IValueReader<T> valueReader)
		{
			Field = field;
			ValueReader = valueReader;
		}

		public override IEnumerable<(ColumnExpression ColumnExpression, QueryExpression ValueExpression)> GetColumnExpressionPairs()
		{
			//  todo: these column expressions should probably have their sources resolved somehow
			foreach (var column in Field.Columns)
			{
				if (!SqlTypeHelper.IsSqlPrimitiveType(Field.DataType))
				{
					if (Field.KeyType == KeyType.None)
					{
						if (ValueReader.Read() == null)
							yield return (QueryExpression.Column(column.ColumnName), QueryExpression.Value(false));
						else
							yield return (QueryExpression.Column(column.ColumnName), QueryExpression.Value(true));
					}
					else
					{
						var foreignKey = Field.ForeignKeys.First(q => q.LocalColumn == column);
						var obj = ValueReader.Read();
						var objectReadWriter = new ObjectReadWriter(obj, Field.DataTypeModel, Field.DataType);

						yield return (QueryExpression.Column(column.ColumnName), QueryExpression.Value(
							foreignKey.ReadValue(objectReadWriter, Field.FieldReference.Length)
							));
					}
				}
				else
				{
					yield return (QueryExpression.Column(column.ColumnName), QueryExpression.Value(ValueReader.Read()));
				}
			}
		}
	}
}
