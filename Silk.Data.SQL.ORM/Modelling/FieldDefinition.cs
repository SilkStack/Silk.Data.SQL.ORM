using Silk.Data.Modelling.Bindings;
using System;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Modelling
{
	/// <summary>
	/// Describes a field.
	/// </summary>
	public class FieldDefinition
	{
		/// <summary>
		/// Gets or sets the name of the field.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Gets or sets the model binding.
		/// </summary>
		public ModelBinding Binding { get; set; }
		/// <summary>
		/// Gets or sets the field opinions.
		/// </summary>
		public FieldOpinions Opinions { get; set; }
		/// <summary>
		/// Gets or sets the SQL data type.
		/// </summary>
		public SqlDataType SqlDataType { get; set; }
		/// <summary>
		/// Gets or sets the CLR <see cref="Type"/> of the field.
		/// </summary>
		public Type ClrType { get; set; }
		/// <summary>
		/// Gets or sets a value indicating if the field is a primary key.
		/// </summary>
		public bool IsPrimaryKey { get; set; }
		/// <summary>
		/// Gets or sets a value indicating if the field's value should be auto generated.
		/// </summary>
		/// <remarks>Requires <see cref="IsPrimaryKey"/> to be true.</remarks>
		public bool AutoGenerate { get; set; }
		/// <summary>
		/// Gets or sets a value indicating if the field should be indexed.
		/// </summary>
		public bool IsIndex { get; set; }
		/// <summary>
		/// Gets or sets a value indicating if the field should have a UNIQUE constraint.
		/// </summary>
		/// <remarks>Requires <see cref="IsIndex"/> to be true.</remarks>
		public bool IsUnique { get; set; }
		/// <summary>
		/// Gets or sets a value indicating if the field is nullable.
		/// </summary>
		public bool IsNullable { get; set; }

		public static FieldDefinition SimpleMappedField(string name, ModelBinding binding, FieldOpinions opinions,
			SqlDataType sqlDataType, Type clrType)
		{
			return new FieldDefinition
			{
				Name = name,
				Binding = binding,
				Opinions = opinions,
				SqlDataType = sqlDataType,
				ClrType = clrType,
				IsPrimaryKey = opinions.IsPrimaryKey,
				AutoGenerate = opinions.AutoGenerate,
				IsIndex = opinions.IsIndex,
				IsUnique = opinions.IsUnique,
				IsNullable = TypeIsNullable(clrType)
			};
		}

		private static bool TypeIsNullable(Type t)
		{
			var typeInfo = t.GetTypeInfo();
			var isNullable = !typeInfo.IsValueType;
			if (!isNullable)
			{
				isNullable = typeInfo.IsGenericType &&
					t.GetGenericTypeDefinition() == typeof(Nullable<>);
			}
			return isNullable;
		}
	}
}
