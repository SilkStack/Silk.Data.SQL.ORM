using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Modelling.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM
{
	/// <summary>
	/// Builder API to help construct functioning DataDomain instances.
	/// </summary>
	public class DataDomainBuilder
	{
		private static ViewConvention[] _defaultViewConventions
			= new ViewConvention[]
			{
				new CleanModelNameConvention(),
				new CopyPrimitiveTypesConvention(),
				new IdIsPrimaryKeyConvention(),
				new ManyToOneConvention(),
				new ManyToManyConvention(),
				new FlattenPocosConvention()
			};

		private readonly List<EntityModelBuilder> _entityModelBuilders = new List<EntityModelBuilder>();
		private readonly DomainDefinition _domainDefinition = new DomainDefinition();
		private readonly ViewConvention[] _viewConventions;

		public DataDomainBuilder()
			: this(_defaultViewConventions)
		{
		}

		public DataDomainBuilder(IEnumerable<ViewConvention> viewConventions)
		{
			_viewConventions = viewConventions.ToArray();
		}

		public void AddDataEntity<TSource>(
			Action<EntityModel<TSource>> builtDelegate = null,
			Action<ModelCustomizer<TSource>> customizeModelDelegate = null
			)
			where TSource : new()
		{
			_domainDefinition.EntityTypes.Add(typeof(TSource));
			_entityModelBuilders.Add(new EntityModelBuilder<TSource>(
				builtDelegate, _viewConventions, _domainDefinition, customizeModelDelegate
				));
		}

		[Obsolete("Doesn't support full API and will be removed when consumer code has been updated to use AddDataEntity<TSource> only.")]
		public void AddDataEntity<TSource, TView>(Action<EntityModel<TSource, TView>> builtDelegate = null)
			where TSource : new()
			where TView : new()
		{
			_domainDefinition.EntityTypes.Add(typeof(TSource));
			_entityModelBuilders.Add(new EntityModelBuilder<TSource, TView>(
				builtDelegate, _viewConventions, _domainDefinition
				));
		}

		public DataDomain Build()
		{
			var viewDefinitionFieldCounts = new Dictionary<ViewDefinition, int>();
			var currentViewDefinitionFieldCounts = new Dictionary<ViewDefinition, int>();
			DataDomain builtDomain = null;
			var lazyDomainAccessor = new Lazy<DataDomain>(() => builtDomain);

			foreach (var entityModelBuilder in _entityModelBuilders)
			{
				viewDefinitionFieldCounts.Add(entityModelBuilder.ViewDefinition, 0);
			}

			var fieldsChanged = true;
			while (fieldsChanged)
			{
				fieldsChanged = false;

				foreach (var entityModelBuilder in _entityModelBuilders)
				{
					entityModelBuilder.ViewBuilder.ProcessModel(entityModelBuilder.ViewDefinition.TargetModel);

					currentViewDefinitionFieldCounts[entityModelBuilder.ViewDefinition] = entityModelBuilder.ViewDefinition.FieldDefinitions.Count;
					if (currentViewDefinitionFieldCounts[entityModelBuilder.ViewDefinition] !=
						viewDefinitionFieldCounts[entityModelBuilder.ViewDefinition])
						fieldsChanged = true;
					viewDefinitionFieldCounts[entityModelBuilder.ViewDefinition] = entityModelBuilder.ViewDefinition.FieldDefinitions.Count;

					entityModelBuilder.ViewBuilder.IsFirstPass = false;
				}
			}
			foreach (var entityModelBuilder in _entityModelBuilders)
			{
				entityModelBuilder.ViewBuilder.FinalizeModel();
			}

			foreach (var entityModelBuilder in _entityModelBuilders)
			{
				entityModelBuilder.CustomizeModel();
			}

			builtDomain = DataDomain.CreateFromDefinition(_domainDefinition, _viewConventions);

			foreach (var entityModelBuilder in _entityModelBuilders)
			{
				entityModelBuilder.CallBuiltDelegate(builtDomain);
			}

			return builtDomain;
		}

		private abstract class EntityModelBuilder
		{
			public abstract DataViewBuilder ViewBuilder { get; }
			public abstract Type ModelType { get; }
			public abstract Type ProjectionType { get; }
			public ViewDefinition ViewDefinition { get; protected set; }
			public DomainDefinition DomainDefinition { get; protected set; }

			public abstract void CallBuiltDelegate(DataDomain dataDomain);
			public abstract void CustomizeModel();
		}

		private class EntityModelBuilder<TSource> : EntityModelBuilder
			where TSource : new()
		{
			private readonly Action<EntityModel<TSource>> _builtDelegate;

			public override Type ModelType => typeof(TSource);
			public override Type ProjectionType => null;
			public override DataViewBuilder ViewBuilder { get; }
			private Action<ModelCustomizer<TSource>> _modelCustomizerFunc;

			public EntityModelBuilder(Action<EntityModel<TSource>> builtDelegate,
				ViewConvention[] viewConventions, DomainDefinition domainDefinition,
				Action<ModelCustomizer<TSource>> modelCustomizerFunc)
			{
				ViewBuilder = new DataViewBuilder(
					TypeModeller.GetModelOf<TSource>(), null,
					viewConventions, domainDefinition,
					typeof(TSource)
					);

				_builtDelegate = builtDelegate;
				ViewDefinition = ViewBuilder.ViewDefinition;
				_modelCustomizerFunc = modelCustomizerFunc;
			}

			public override void CallBuiltDelegate(DataDomain dataDomain)
			{
				_builtDelegate?.Invoke(dataDomain.GetEntityModel<TSource>());
			}

			public override void CustomizeModel()
			{
				_modelCustomizerFunc?.Invoke(new ModelCustomizer<TSource>(ViewBuilder));
			}
		}

		private class EntityModelBuilder<TSource,TView> : EntityModelBuilder
			where TSource : new()
			where TView : new()
		{
			private readonly Action<EntityModel<TSource, TView>> _builtDelegate;

			public override Type ModelType => typeof(TSource);
			public override Type ProjectionType => typeof(TView);
			public override DataViewBuilder ViewBuilder { get; }

			public EntityModelBuilder(Action<EntityModel<TSource, TView>> builtDelegate,
				ViewConvention[] viewConventions, DomainDefinition domainDefinition)
			{
				ViewBuilder = new DataViewBuilder(
					TypeModeller.GetModelOf<TSource>(), 
					TypeModeller.GetModelOf<TView>(),
					viewConventions, domainDefinition,
					typeof(TSource), typeof(TView)
					);

				_builtDelegate = builtDelegate;
				ViewDefinition = ViewBuilder.ViewDefinition;
			}

			public override void CallBuiltDelegate(DataDomain dataDomain)
			{
				if (_builtDelegate != null)
					_builtDelegate(dataDomain.GetEntityModel<TSource>() as EntityModel<TSource, TView>);
			}

			public override void CustomizeModel()
			{
			}
		}
	}
}
