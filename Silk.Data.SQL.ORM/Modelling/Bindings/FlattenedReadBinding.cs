//using Silk.Data.Modelling;
//using Silk.Data.Modelling.Bindings;
//using System;
//using System.Linq;

//namespace Silk.Data.SQL.ORM.Modelling.Bindings
//{
//	public class FlattenedReadBinding : ModelBinding
//	{
//		public override BindingDirection Direction => BindingDirection.ViewToModel;

//		private string _flatModelPath;

//		public FlattenedReadBinding(string[] modelFieldPath, string[] viewFieldPath)
//			: base(modelFieldPath, viewFieldPath)
//		{
//			_flatModelPath = string.Join("", modelFieldPath);
//		}

//		public override void WriteToModel(IModelReadWriter modelReadWriter, object value, MappingContext mappingContext)
//		{
//			var field = modelReadWriter.Model.Fields.FirstOrDefault(q => q.Name == _flatModelPath);
//			if (field == null)
//				throw new InvalidOperationException("Invalid field path.");
//			modelReadWriter = modelReadWriter.GetField(field);
//			if (modelReadWriter == null)
//				throw new InvalidOperationException($"Couldn't get field \"{field.Name}\".");
//			modelReadWriter.Value = value;
//		}
//	}
//}
