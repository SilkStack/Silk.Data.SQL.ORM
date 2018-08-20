using Silk.Data.Modelling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Configures and builds an entity field.
	/// </summary>
	public abstract class EntityFieldBuilder
	{
		/// <summary>
		/// Gets the model field the field builder represents.
		/// </summary>
		public abstract IPropertyField ModelField { get; }

		/// <summary>
		/// Builds the entity field.
		/// </summary>
		/// <returns>Null when the field shouldn't be stored in the schema being built.</returns>
		public abstract EntityField Build();
	}

	/// <summary>
	/// Configures and builds an entity field of type T.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EntityFieldBuilder<T> : EntityFieldBuilder
	{
		/// <summary>
		/// Gets the model field the field builder represents.
		/// </summary>
		public override IPropertyField ModelField { get; }

		public EntityFieldBuilder(IPropertyField modelField)
		{
			ModelField = modelField;
		}

		public override EntityField Build()
		{
			if (!ModelField.CanRead || ModelField.IsEnumerable)
				return null;

			return new EntityField<T>(new Column());
		}
	}
}
