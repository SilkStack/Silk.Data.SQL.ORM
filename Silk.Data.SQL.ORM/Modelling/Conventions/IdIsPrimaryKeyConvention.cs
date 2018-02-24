using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	public class IdIsPrimaryKeyConvention : ISchemaConvention
	{
		public void VisitModel(TypedModel model, SchemaBuilder builder)
		{
			if (!builder.IsAtContextRoot)
				return;

			var entityDefinition = builder.GetEntityDefinition(model.DataType);
			if (ModelHasPrimaryKey(entityDefinition))
				return;

			var idFieldDefinition = entityDefinition.Fields.FirstOrDefault(q => q.Name == "Id");
			if (idFieldDefinition == null)
				return;

			idFieldDefinition.IsPrimaryKey = true;
			if (idFieldDefinition.ClrType == typeof(int) ||
				idFieldDefinition.ClrType == typeof(long) ||
				idFieldDefinition.ClrType == typeof(short) ||
				idFieldDefinition.ClrType == typeof(Guid))
				idFieldDefinition.AutoGenerate = true;
		}

		private bool ModelHasPrimaryKey(EntityDefinition entityDefinition)
		{
			return entityDefinition.Fields.Any(q => q.IsPrimaryKey);
		}
	}
}
