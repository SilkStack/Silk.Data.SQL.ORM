using Silk.Data.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Modelling
{
	/// <summary>
	/// Customizes how an entity type will be modelled.
	/// </summary>
	public abstract class ModelCustomizer
	{
		/// <summary>
		/// Gets the entity Type.
		/// </summary>
		public abstract Type EntityType { get; }
		/// <summary>
		/// Gets a model of the entity Type.
		/// </summary>
		public abstract TypedModel EntityModel { get; }

		/// <summary>
		/// Gets all the field customizers for the model.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<FieldCustomizer> GetFieldCustomizers();
	}

	/// <summary>
	/// Customizes how an entity type will be modelled.
	/// </summary>
	public class ModelCustomizer<TSource> : ModelCustomizer
		where TSource : new()
	{
		public override Type EntityType => EntityModel.DataType;
		public override TypedModel EntityModel { get; }

		private readonly Dictionary<string, FieldCustomizer> _fieldCustomizers
			= new Dictionary<string, FieldCustomizer>();

		public ModelCustomizer(TypedModel entityModel)
		{
			EntityModel = entityModel;
		}

		/// <summary>
		/// Gets a field customizer for a field.
		/// </summary>
		/// <typeparam name="TField"></typeparam>
		/// <param name="fieldSelector"></param>
		/// <returns></returns>
		public FieldCustomizer<TField> For<TField>(Expression<Func<TSource, TField>> fieldSelector)
		{
			if (fieldSelector.Body is MemberExpression memberExpression)
			{
				var memberName = memberExpression.Member.Name;

				FieldCustomizer fieldCustomizer;
				if (_fieldCustomizers.TryGetValue(memberName, out fieldCustomizer))
					return fieldCustomizer as FieldCustomizer<TField>;

				var field = EntityModel.Fields.FirstOrDefault(q => q.Name == memberName);
				if (field == null)
					throw new ArgumentException("Field selector expression doesn't specify a valid member.", nameof(fieldSelector));

				fieldCustomizer = new FieldCustomizer<TField>(field);
				_fieldCustomizers.Add(memberName, fieldCustomizer);
				return fieldCustomizer as FieldCustomizer<TField>;
			}
			throw new ArgumentException("Field selector must be a MemberExpression.", nameof(fieldSelector));
		}

		public override IEnumerable<FieldCustomizer> GetFieldCustomizers()
		{
			return _fieldCustomizers.Values;
		}
	}
}
