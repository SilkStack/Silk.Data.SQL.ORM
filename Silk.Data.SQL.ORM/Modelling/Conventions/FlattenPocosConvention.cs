﻿using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class FlattenPocosConvention : ViewConvention<DataViewBuilder>
	{
		public override ViewType SupportedViewTypes => ViewType.All;
		public override bool PerformMultiplePasses => false;
		public override bool SkipIfFieldDefined => true;

		public override void MakeModelField(DataViewBuilder viewBuilder, ModelField field)
		{
			if (viewBuilder.Mode == ViewType.ConventionDerived)
				MakeConventionDerivedModelField(viewBuilder, field);
			else if (viewBuilder.Mode == ViewType.ModelDriven)
				MakeModelDrivenModelField(viewBuilder, field);
		}

		private void MakeConventionDerivedModelField(DataViewBuilder viewBuilder, ModelField field)
		{
			if (viewBuilder.IsPrimitiveType(field.DataType) ||
				viewBuilder.DomainDefinition.EntityTypes.Contains(field.DataType))
				return;

			viewBuilder.PushModel(field.Name, field.DataTypeModel);
			try
			{
				viewBuilder.ProcessModel(field.DataTypeModel);
			}
			finally
			{
				viewBuilder.PopModel();
			}
		}

		private void MakeModelDrivenModelField(DataViewBuilder viewBuilder, ModelField field)
		{
			if (!viewBuilder.IsPrimitiveType(field.DataType))
				return;

			var modelBindPath = field.Name.Split('_');
			var sourceField = viewBuilder.FindSourceField(field, modelBindPath,
				dataType: field.DataType);

			if (sourceField == null)
				return;

			viewBuilder.DefineAssignedViewField(sourceField, modelBindPath, field.Name, field.Metadata);
		}
	}
}
