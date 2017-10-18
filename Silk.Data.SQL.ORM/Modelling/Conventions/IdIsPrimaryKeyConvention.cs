using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class IdIsPrimaryKeyConvention : ViewConvention
	{
		public override void FinalizeModel(ViewDefinition viewDefinition)
		{
			var hasPrimaryKey = viewDefinition.FieldDefinitions.Any(
				fieldDef => fieldDef.Metadata.OfType<PrimaryKeyAttribute>().Any()
				);
			if (hasPrimaryKey)
				return;
			var idField = viewDefinition.FieldDefinitions.FirstOrDefault(
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
