using Silk.Data.Modelling;

namespace Silk.Data.SQL.ORM.Schema
{
	public class RelationshipFieldBuilder
	{
		/// <summary>
		/// Gets the model field the field builder represents.
		/// </summary>
		public IPropertyField ModelField { get; }

		/// <summary>
		/// Gets or sets the column name to use to store the field.
		/// </summary>
		public string ColumnName { get; set; }

		public RelationshipFieldBuilder(IPropertyField modelField)
		{
			ModelField = modelField;
		}
	}
}
