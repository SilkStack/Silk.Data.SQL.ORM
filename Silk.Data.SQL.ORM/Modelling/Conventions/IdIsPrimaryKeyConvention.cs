using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class IdIsPrimaryKeyConvention : ViewConvention<DataViewBuilder>
	{
		public override ViewType SupportedViewTypes => ViewType.All;
		public override bool PerformMultiplePasses => true;
		public override bool SkipIfFieldDefined => false;

		public override void FinalizeModel(DataViewBuilder viewBuilder)
		{
			var hasPrimaryKey = viewBuilder.ViewDefinition.FieldDefinitions.Any(
				fieldDef => fieldDef.Metadata.OfType<PrimaryKeyAttribute>().Any()
				);
			if (hasPrimaryKey)
				return;
			var idField = viewBuilder.ViewDefinition.FieldDefinitions.FirstOrDefault(
				fieldDef => fieldDef.Name == "Id"
				);
			if (idField == null)
				return;

			idField.Metadata.Add(new PrimaryKeyAttribute());
			if (idField.DataType == typeof(int) ||
				idField.DataType == typeof(long) ||
				idField.DataType == typeof(short))
				idField.Metadata.Add(new AutoIncrementAttribute());
			else if (idField.DataType == typeof(Guid))
				idField.Metadata.Add(new AutoGenerateIdAttribute());
		}
	}
}
