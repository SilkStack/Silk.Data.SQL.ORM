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
		private static ViewConvention[] _defaultViewConventions = new ViewConvention[]
		{
			new CleanModelNameConvention(),
			new CopySupportedSQLTypesConvention(),
			new IdIsPrimaryKeyConvention(),
			new CopyPrimaryKeyOfTypesWithSchemaConvention(),
			new ProjectReferenceKeysConvention()
		};
		private static EnumerableConversionsConvention _bindEnumerableConversions = new EnumerableConversionsConvention();

		private readonly List<EntityModelBuilder> _entityModelBuilders = new List<EntityModelBuilder>();
		private readonly ViewConvention[] _viewConventions;
		private readonly DomainDefinition _domainDefinition = new DomainDefinition();

		public DataDomainBuilder() : this(_defaultViewConventions)
		{
		}

		public DataDomainBuilder(IEnumerable<ViewConvention> viewConventions)
		{
			_viewConventions = viewConventions.ToArray();
		}

		public void AddDataEntity<TSource>(Action<EntityModel<TSource>> builtDelegate = null)
			where TSource : new()
		{
			_entityModelBuilders.Add(new EntityModelBuilder<TSource>(
				builtDelegate, _viewConventions, _domainDefinition
				));
		}

		public void AddDataEntity<TSource, TView>(Action<EntityModel<TSource, TView>> builtDelegate = null)
			where TSource : new()
			where TView : new()
		{
			_entityModelBuilders.Add(new EntityModelBuilder<TSource, TView>(
				builtDelegate, _viewConventions, _domainDefinition
				));
		}

		public DataDomain Build()
		{
			var viewDefinitionFieldCounts = new Dictionary<ViewDefinition,int>();
			var currentViewDefinitionFieldCounts = new Dictionary<ViewDefinition, int>();
			DataDomain builtDomain = null;
			var lazyDomainAccessor = new Lazy<DataDomain>(() => builtDomain);

			foreach (var entityModelBuilder in _entityModelBuilders)
			{
				entityModelBuilder.ViewDefinition.UserData.Add(lazyDomainAccessor);
				viewDefinitionFieldCounts.Add(entityModelBuilder.ViewDefinition, 0);
			}

			var fieldsChanged = true;
			while (fieldsChanged)
			{
				fieldsChanged = false;

				foreach (var entityModelBuilder in _entityModelBuilders)
				{
					foreach (var field in entityModelBuilder.ViewDefinition.TargetModel.Fields)
					{
						foreach (var viewConvention in _viewConventions)
						{
							viewConvention.MakeModelFields(entityModelBuilder.ViewDefinition.SourceModel,
								field, entityModelBuilder.ViewDefinition);
						}
					}

					_bindEnumerableConversions.FinalizeModel(entityModelBuilder.ViewDefinition);

					foreach (var viewConvention in _viewConventions)
					{
						viewConvention.FinalizeModel(entityModelBuilder.ViewDefinition);
					}

					currentViewDefinitionFieldCounts[entityModelBuilder.ViewDefinition] = entityModelBuilder.ViewDefinition.FieldDefinitions.Count;
					if (currentViewDefinitionFieldCounts[entityModelBuilder.ViewDefinition] !=
						viewDefinitionFieldCounts[entityModelBuilder.ViewDefinition])
						fieldsChanged = true;
					viewDefinitionFieldCounts[entityModelBuilder.ViewDefinition] = entityModelBuilder.ViewDefinition.FieldDefinitions.Count;
				}
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
			public abstract Type ModelType { get; }
			public abstract Type Projectiontype { get; }
			public ViewDefinition ViewDefinition { get; protected set; }
			public DomainDefinition DomainDefinition { get; protected set; }

			public abstract void CallBuiltDelegate(DataDomain dataDomain);
		}

		private class EntityModelBuilder<TSource> : EntityModelBuilder
			where TSource : new()
		{
			private readonly Action<EntityModel<TSource>> _builtDelegate;

			public override Type ModelType => typeof(TSource);

			public override Type Projectiontype => null;

			public EntityModelBuilder(Action<EntityModel<TSource>> builtDelegate,
				ViewConvention[] viewConventions, DomainDefinition domainDefinition)
			{
				_builtDelegate = builtDelegate;
				ViewDefinition = new ViewDefinition(
					TypeModeller.GetModelOf<TSource>(),
					TypeModeller.GetModelOf<TSource>(),
					viewConventions
				);
				ViewDefinition.UserData.Add(domainDefinition);
				ViewDefinition.UserData.Add(typeof(TSource));
			}

			public override void CallBuiltDelegate(DataDomain dataDomain)
			{
				if (_builtDelegate != null)
					_builtDelegate(dataDomain.GetEntityModel<TSource>());
			}
		}

		private class EntityModelBuilder<TSource,TView> : EntityModelBuilder
			where TSource : new()
			where TView : new()
		{
			private readonly Action<EntityModel<TSource, TView>> _builtDelegate;

			public override Type ModelType => typeof(TSource);
			public override Type Projectiontype => typeof(TView);

			public EntityModelBuilder(Action<EntityModel<TSource, TView>> builtDelegate,
				ViewConvention[] viewConventions, DomainDefinition domainDefinition)
			{
				_builtDelegate = builtDelegate;
				ViewDefinition = new ViewDefinition(
					TypeModeller.GetModelOf<TSource>(),
					TypeModeller.GetModelOf<TView>(),
					viewConventions
				);
				ViewDefinition.UserData.Add(domainDefinition);
				ViewDefinition.UserData.Add(typeof(TSource));
				ViewDefinition.UserData.Add(typeof(TView));
			}

			public override void CallBuiltDelegate(DataDomain dataDomain)
			{
				if (_builtDelegate != null)
					_builtDelegate(dataDomain.GetEntityModel<TSource>() as EntityModel<TSource, TView>);
			}
		}
	}
}
