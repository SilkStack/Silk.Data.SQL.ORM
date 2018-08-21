using Silk.Data.Modelling;
using System;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// An entity field.
	/// </summary>
	public abstract class EntityField
	{
		public abstract Type FieldType { get; }
		public abstract Column Column { get; }
		public abstract IPropertyField ModelField { get; }
	}

	/// <summary>
	/// An entity field that stores type T.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EntityField<T> : EntityField
	{
		public override Type FieldType { get; } = typeof(T);
		public override Column Column { get; }
		public override IPropertyField ModelField { get; }

		public EntityField(Column column, IPropertyField modelField)
		{
			Column = column;
			ModelField = modelField;
		}
	}
}
