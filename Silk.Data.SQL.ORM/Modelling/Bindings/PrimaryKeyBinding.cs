using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Bindings
{
	public class PrimaryKeyBinding : ModelBinding
	{
		public PrimaryKeyBinding(BindingDirection bindingDirection, string[] modelFieldPath,
			string[] viewFieldPath, string fieldName) : base(modelFieldPath, viewFieldPath)
		{
			Direction = bindingDirection;
			FieldName = fieldName;
		}

		public override BindingDirection Direction { get; }
		public string FieldName { get; }

		public override object ReadFromContainer(IContainer container, MappingContext mappingContext)
		{
			return mappingContext.Resources.Retrieve(container, FieldName);
		}

		public override void WriteToModel(IModelReadWriter modelReadWriter, object value, MappingContext mappingContext)
		{
			
			var field = modelReadWriter.Model.Fields.FirstOrDefault(q => q.Name == FieldName);
			if (field == null)
				throw new InvalidOperationException("Invalid field path.");
			modelReadWriter = modelReadWriter.GetField(field);
			if (modelReadWriter == null)
				throw new InvalidOperationException($"Couldn't get field \"{field.Name}\".");
			modelReadWriter.Value = value;
		}
	}
}
