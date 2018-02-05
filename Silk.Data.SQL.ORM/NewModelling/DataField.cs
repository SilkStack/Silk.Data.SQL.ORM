using System;
using Silk.Data.Modelling.Bindings;

namespace Silk.Data.SQL.ORM.NewModelling
{
	/// <summary>
	/// Implementation of <see cref="IDataField"/>.
	/// </summary>
	public class DataField : IDataField
	{
		private readonly static object[] _metadata = new object[0];

		/// <summary>
		/// Gets the name of the field as created in the database.
		/// </summary>
		/// <remarks>Does not have to match the property name being mapped.</remarks>
		public string Name { get; }

		/// <summary>
		/// Gets the CLR Type of the field.
		/// </summary>
		public Type DataType { get; }

		public object[] Metadata => _metadata;

		/// <summary>
		/// Gets the model binding used to map data from view to model.
		/// </summary>
		public ModelBinding ModelBinding { get; }

		/// <summary>
		/// Gets a value indicating if the field is a mapped object that resides beyond the entity table.
		/// </summary>
		public bool IsMappedObject { get; }

		public bool IsRelationshipKey { get; }

		/// <summary>
		/// Gets a descriptor for the relationship represented by this field.
		/// </summary>
		public RelationshipDescriptor Relationship { get; }

		public IDataField[] MappedObjectKeyFields { get; }

		public IDataField MappedObjectField { get; }

		public SqlDataType SqlType { get; }

		public Type ClrType { get; }

		public DataField(string name, Type clrDataType, SqlDataType sqlDataType,
			ModelBinding modelBinding)
		{
			Name = name;
			DataType = clrDataType;
			SqlType = sqlDataType;
			ModelBinding = modelBinding;
		}
	}
}
