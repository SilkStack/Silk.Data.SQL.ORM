using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.Bindings
{
	public class ManyRelatedObjectsBinding : ModelBinding
	{
		private readonly Type _dataType;
		private readonly ObjectMapping _mapping;

		public override BindingDirection Direction => BindingDirection.Bidirectional;

		public ManyRelatedObjectsBinding(string[] modelFieldPath, string[] viewFieldPath, Type dataType)
			: base(modelFieldPath, viewFieldPath)
		{
			_dataType = dataType;
			_mapping = Activator.CreateInstance(typeof(ObjectMapping<>).MakeGenericType(dataType)) as ObjectMapping;
		}

		public override T ReadTransformedValue<T>(IContainerReadWriter from, MappingContext mappingContext)
		{
			if (mappingContext.BindingDirection == BindingDirection.ViewToModel)
			{
				if (((ViewReadWriter)from).View is EntityModel fromEntityModel)
				{
					var collectionDictionaries = from.ReadFromPath<List<Dictionary<string, object>>>(ViewFieldPath);
					var viewField = fromEntityModel.Fields.First(q => q.Name == ViewFieldPath[0]);
					return (T)_mapping.MapToModels(collectionDictionaries, viewField.Relationship.ProjectedModel ?? viewField.Relationship.ForeignModel);
				}
			}
			return base.ReadTransformedValue<T>(from, mappingContext);
		}

		private abstract class ObjectMapping
		{
			public abstract IEnumerable MapToModels(List<Dictionary<string, object>> stores, EntityModel view);
		}

		private class ObjectMapping<T> :
			ObjectMapping
			where T : new()
		{
			private readonly TypedModel<T> _model;

			public ObjectMapping()
			{
				_model = TypeModeller.GetModelOf<T>();
			}

			public override IEnumerable MapToModels(List<Dictionary<string, object>> stores,
				EntityModel view)
			{
				if (stores == null)
					return null;

				var viewReadWriters = stores
					.Select(q => new MemoryViewReadWriter(view, q))
					.ToArray();
				var modelInstances = stores
					.Select(q => new T())
					.ToArray();
				var modelReadWriters = modelInstances
					.Select(q => new ObjectModelReadWriter(_model, q))
					.ToArray();

				view.MapToModelAsync(modelReadWriters, viewReadWriters)
					.ConfigureAwait(false)
					.GetAwaiter()
					.GetResult();

				return modelInstances;
			}
		}
	}
}
