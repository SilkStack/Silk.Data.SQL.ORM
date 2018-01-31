using Silk.Data.Modelling;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	/// <summary>
	/// Describes developer opinions on how to model an entity.
	/// </summary>
	public class ModelOpinions
	{
		/// <summary>
		/// Gets a default <see cref="ModelOpinions"/> instance.
		/// </summary>
		public static ModelOpinions Default { get; } = new ModelOpinions();

		public string Name { get; }

		private readonly Dictionary<ModelField, FieldOpinions> _fieldOpinions;

		private ModelOpinions() { }

		public ModelOpinions(string name, Dictionary<ModelField,FieldOpinions> fieldOpinions)
		{
			Name = name;
			_fieldOpinions = fieldOpinions;
		}

		/// <summary>
		/// Gets opinions for a given field.
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		public FieldOpinions GetFieldOpinions(ModelField field)
		{
			if (_fieldOpinions.TryGetValue(field, out var fieldOpinions))
				return fieldOpinions;
			return FieldOpinions.Default;
		}
	}
}
