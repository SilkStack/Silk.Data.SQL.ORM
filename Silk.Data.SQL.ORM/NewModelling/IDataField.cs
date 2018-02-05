using Silk.Data.Modelling;
using System;

namespace Silk.Data.SQL.ORM.NewModelling
{
	/// <summary>
	/// Details a field on a <see cref="IEntitySchema"/>.
	/// </summary>
	public interface IDataField : IViewField
	{
		/// <summary>
		/// Gets a value indicating if the field is a mapped object that resides beyond the entity table.
		/// </summary>
		bool IsMappedObject { get; }

		/// <summary>
		/// Gets an array of fields that are used to store the foreign keys for the mapped object.
		/// </summary>
		IDataField[] MappedObjectKeyFields { get; }

		/// <summary>
		/// Gets a value indicating if this field is a relationship key.
		/// </summary>
		bool IsRelationshipKey { get; }

		/// <summary>
		/// Gets the field this foreign key stores.
		/// </summary>
		IDataField MappedObjectField { get; }

		/// <summary>
		/// Gets a descriptor for the relationship represented by this field.
		/// </summary>
		RelationshipDescriptor Relationship { get; }

		/// <summary>
		/// Gets the field's SQL data type.
		/// </summary>
		SqlDataType SqlType { get; }

		/// <summary>
		/// Gets the field's native CLR type.
		/// </summary>
		Type ClrType { get; }
	}
}
