using Silk.Data.Modelling.Mapping.Binding;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Describs how a field is mapped from schema to entity type.
	/// </summary>
	public abstract class ProjectionField : IProjectedItem
	{
		/// <summary>
		/// Gets the table name/alias that the field is a member of.
		/// </summary>
		public string SourceName { get; }
		/// <summary>
		/// Gets the name of the field.
		/// </summary>
		public string FieldName { get; }
		/// <summary>
		/// Gets the alias the field should be projected as.
		/// </summary>
		public string AliasName { get; }
		/// <summary>
		/// Gets the path to the property on the entity model.
		/// </summary>
		public string[] ModelPath { get; }

		public ProjectionField(string sourceName, string fieldName, string aliasName,
			string[] modelPath)
		{
			SourceName = sourceName;
			FieldName = fieldName;
			AliasName = aliasName;
			ModelPath = modelPath;
		}

		public abstract Binding GetMappingBinding();
	}

	public class ProjectionField<T> : ProjectionField
	{
		public ProjectionField(string sourceName, string fieldName, string aliasName, string[] modelPath)
			: base(sourceName, fieldName, aliasName, modelPath)
		{
		}

		public override Binding GetMappingBinding()
		{
			return new CopyBinding<T>(new[] { AliasName }, ModelPath);
		}
	}
}
