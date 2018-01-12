using Silk.Data.Modelling;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class FieldCustomizer<TField>
	{
		private readonly DataViewBuilder _viewBuilder;
		private readonly ViewFieldDefinition _fieldDefinition;

		public FieldCustomizer(DataViewBuilder viewBuilder, ViewFieldDefinition fieldDefinition)
		{
			_viewBuilder = viewBuilder;
			_fieldDefinition = fieldDefinition;
		}

		public FieldCustomizer<TField> IsPrimaryKey()
		{
			foreach (var fieldDefinition in _viewBuilder.ViewDefinition.FieldDefinitions)
			{
				fieldDefinition.Metadata.RemoveAll(q => q is PrimaryKeyAttribute);
			}
			_fieldDefinition.Metadata.Add(new PrimaryKeyAttribute());
			return this;
		}

		public FieldCustomizer<TField> SqlType(SqlDataType dataType)
		{
			_fieldDefinition.Metadata.RemoveAll(q => q is SqlDataType);
			_fieldDefinition.Metadata.Add(dataType);
			return this;
		}
	}
}
