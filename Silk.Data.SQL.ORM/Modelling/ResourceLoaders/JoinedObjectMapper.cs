using System.Collections.Generic;
using System.Threading.Tasks;
using Silk.Data.Modelling;
using Silk.Data.Modelling.ResourceLoaders;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling.ResourceLoaders
{
	public class JoinedObjectMapper : IResourceLoader
	{
		private readonly DataDomain _domain;
		private readonly Model _model;
		private readonly List<Mapping> _mappings = new List<Mapping>();

		public JoinedObjectMapper(Model model, DataDomain domain)
		{
			_model = model;
			_domain = domain;
		}

		public Mapping GetMapping(Type type)
		{
			var mapping = _mappings.FirstOrDefault(q => q.Type == type);
			if (mapping != null)
				return mapping;
			mapping = new Mapping(type, _model, _domain);
			_mappings.Add(mapping);
			return mapping;
		}

		public async Task LoadResourcesAsync(ICollection<IContainer> containers, MappingContext mappingContext)
		{
			foreach (var mapping in _mappings)
			{
				await mapping.PerformMapping(containers, mappingContext)
					.ConfigureAwait(false);
			}
		}

		public Task LoadResourcesAsync(ICollection<IModelReadWriter> modelReadWriters, MappingContext mappingContext)
		{
			//  noop
			return Task.CompletedTask;
		}

		public class Mapping
		{
			private readonly List<(string FieldName, Type ProjectedType)> _fields
				= new List<(string FieldName, Type ProjectedType)>();
			private EntityModel _dataModel;

			public Type Type { get; }
			public DataDomain Domain { get; }
			public Model ParentModel { get; }

			public Mapping(Type type,
				Model model, DataDomain domain)
			{
				Type = type;
				Domain = domain;
				ParentModel = model;
			}

			public void AddField(string fieldName, Type projectedType)
			{
				_fields.Add((fieldName, projectedType));
			}

			public async Task PerformMapping(ICollection<IContainer> containers, MappingContext mappingContext)
			{
				if (_dataModel == null)
					_dataModel = Domain.DataModels.FirstOrDefault(q => q.EntityType == Type);
				if (_dataModel == null)
					throw new InvalidOperationException($"No data model in domain for type {Type.FullName}.");

				//  make a container and readwriter for each field
				var readWriters = new List<IModelReadWriter>();
				var entityContainers = new List<IContainer>();
				foreach (var container in containers)
				{
					foreach (var fieldTuple in _fields)
					{
						var field = fieldTuple.FieldName;
						var type = fieldTuple.ProjectedType;
						//  todo: don't use Activator, use a compiled expression
						var result = Activator.CreateInstance(type);
						readWriters.Add(
							new ObjectReadWriter(type, _dataModel.Model, result)
							);
						entityContainers.Add(
							new ProxyContainer(_dataModel.Model as TypedModel, _dataModel,
								$"{field}_", container, result)
							);
					}
				}

				await _dataModel.MapToModelAsync(readWriters, entityContainers)
					.ConfigureAwait(false);

				foreach (var container in containers)
				{
					foreach (var fieldTuple in _fields)
					{
						var field = fieldTuple.FieldName;
						var entityContainer = entityContainers
							.OfType<ProxyContainer>()
							.FirstOrDefault(q => q.Prefix == $"{field}_" && q.BaseContainer == container);
						mappingContext.Resources.Store(container, field, entityContainer.ValueReference);
					}
				}
			}
		}

		private class ProxyContainer : IContainer
		{
			public TypedModel Model { get; }

			public IView View { get; }

			public string Prefix { get; }

			public IContainer BaseContainer { get; }

			public object ValueReference { get; }

			public ProxyContainer(TypedModel model, IView view,
				string prefix, IContainer baseContainer, object valueReference)
			{
				Model = model;
				View = view;
				Prefix = prefix;
				BaseContainer = baseContainer;
				ValueReference = valueReference;
			}

			public object GetValue(string[] fieldPath)
			{
				if (fieldPath.Length != 1)
					throw new ArgumentOutOfRangeException(nameof(fieldPath), "Field path must have a length of 1.");
				return BaseContainer.GetValue(new[] { $"{Prefix}{fieldPath[0]}" });
			}

			public void SetValue(string[] fieldPath, object value)
			{
				throw new NotSupportedException();
			}
		}
	}
}
