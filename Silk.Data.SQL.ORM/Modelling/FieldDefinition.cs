using Silk.Data.Modelling.Bindings;
using System;

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

		public static FieldDefinition SimpleMappedField(string name, ModelBinding binding, FieldOpinions opinions,
			SqlDataType sqlDataType, Type clrType)
		{
			return new FieldDefinition
			{
				Name = name,
				Binding = binding,
				Opinions = opinions,
				SqlDataType = sqlDataType,
				ClrType = clrType
			};
		}
	}
}
