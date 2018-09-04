using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System.Collections.Generic;
using System.Linq;

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
				if (!SqlTypeHelper.IsSqlPrimitiveType(Field.DataType))
				{
					if (Field.KeyType == KeyType.None)
					{
						if (ValueReader.Read() == null)
							yield return (column, QueryExpression.Value(false));
						else
							yield return (column, QueryExpression.Value(true));
					}
					else
					{
						var foreignKey = Field.ForeignKeys.First(q => q.LocalColumn == column);
						var obj = ValueReader.Read();
						var objectReadWriter = new ObjectReadWriter(obj, Field.ModelField.FieldTypeModel, Field.ModelField.FieldType);

						yield return (column, QueryExpression.Value(
							foreignKey.ReadValue(objectReadWriter, Field.ModelPath.Length)
							));
					}
				}
				else
				{
					yield return (column, QueryExpression.Value(ValueReader.Read()));
				}
			}
		}
	}
}
