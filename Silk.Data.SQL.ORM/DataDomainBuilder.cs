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
		private static ViewConvention<DataViewBuilder>[] _dataViewConventions
			= new ViewConvention<DataViewBuilder>[]
			{
				new IdIsPrimaryKeyConvention()
				//new CleanModelNameConvention(),
				//new CopySupportedSQLTypesConvention(),
				//new IdIsPrimaryKeyConvention(),
				//new CopyPrimaryKeyOfTypesWithSchemaConvention(),
				//new ProjectReferenceKeysConvention()
			};
		private static ViewConvention<ViewBuilder>[] _standardViewConventions
			= new ViewConvention<ViewBuilder>[]
			{
				new CleanModelNameConvention(),
				new CopySimpleTypesConvention()
			};
		private static EnumerableConversionsConvention _bindEnumerableConversions = new EnumerableConversionsConvention();

		private readonly List<EntityModelBuilder> _entityModelBuilders = new List<EntityModelBuilder>();
		private readonly DomainDefinition _domainDefinition = new DomainDefinition();
		private readonly ViewConvention[] _allViewConventions = _standardViewConventions
			.Concat<ViewConvention>(_dataViewConventions).ToArray();

		public DataDomainBuilder()
		{
		}

		public void AddDataEntity<TSource>(Action<EntityModel<TSource>> builtDelegate = null)
			where TSource : new()
		{
			_entityModelBuilders.Add(new EntityModelBuilder<TSource>(
				builtDelegate, _allViewConventions, _domainDefinition
				));
		}

		public void AddDataEntity<TSource, TView>(Action<EntityModel<TSource, TView>> builtDelegate = null)
			where TSource : new()
			where TView : new()
		{
			_entityModelBuilders.Add(new EntityModelBuilder<TSource, TView>(
				builtDelegate, _allViewConventions, _domainDefinition
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
			var firstPass = true;
			while (fieldsChanged)
			{
				fieldsChanged = false;

				foreach (var entityModelBuilder in _entityModelBuilders)
				{
					foreach (var field in entityModelBuilder.ViewDefinition.TargetModel.Fields)
					{
						foreach (var viewConvention in _standardViewConventions)
						{
							if (!firstPass && !viewConvention.PerformMultiplePasses)
								continue;
							if (!viewConvention.SupportedViewTypes.HasFlag(entityModelBuilder.ViewBuilder.Mode))
								continue;
							if (viewConvention.SkipIfFieldDefined &&
								entityModelBuilder.ViewDefinition.FieldDefinitions.Any(q => q.Name == field.Name))
								continue;
							viewConvention.MakeModelField(entityModelBuilder.ViewBuilder, field);
						}
						foreach (var viewConvention in _dataViewConventions)
						{
							if (!firstPass && !viewConvention.PerformMultiplePasses)
								continue;
							if (!viewConvention.SupportedViewTypes.HasFlag(entityModelBuilder.ViewBuilder.Mode))
								continue;
							if (viewConvention.SkipIfFieldDefined &&
								entityModelBuilder.ViewDefinition.FieldDefinitions.Any(q => q.Name == field.Name))
								continue;
							viewConvention.MakeModelField(entityModelBuilder.ViewBuilder, field);
						}
					}

					_bindEnumerableConversions.FinalizeModel(entityModelBuilder.ViewBuilder);

					foreach (var viewConvention in _standardViewConventions)
					{
						if (!firstPass && !viewConvention.PerformMultiplePasses)
							continue;
						viewConvention.FinalizeModel(entityModelBuilder.ViewBuilder);
					}
					foreach (var viewConvention in _dataViewConventions)
					{
						if (!firstPass && !viewConvention.PerformMultiplePasses)
							continue;
						viewConvention.FinalizeModel(entityModelBuilder.ViewBuilder);
					}

					currentViewDefinitionFieldCounts[entityModelBuilder.ViewDefinition] = entityModelBuilder.ViewDefinition.FieldDefinitions.Count;
					if (currentViewDefinitionFieldCounts[entityModelBuilder.ViewDefinition] !=
						viewDefinitionFieldCounts[entityModelBuilder.ViewDefinition])
						fieldsChanged = true;
					viewDefinitionFieldCounts[entityModelBuilder.ViewDefinition] = entityModelBuilder.ViewDefinition.FieldDefinitions.Count;
				}

				firstPass = false;
			}

			builtDomain = DataDomain.CreateFromDefinition(_domainDefinition,
				_standardViewConventions.Concat<ViewConvention>(_dataViewConventions));

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
		}

		private class EntityModelBuilder<TSource> : EntityModelBuilder
			where TSource : new()
		{
			private readonly Action<EntityModel<TSource>> _builtDelegate;

			public override Type ModelType => typeof(TSource);
			public override Type ProjectionType => null;
			public override DataViewBuilder ViewBuilder { get; }

			public EntityModelBuilder(Action<EntityModel<TSource>> builtDelegate,
				ViewConvention[] viewConventions, DomainDefinition domainDefinition)
			{
				ViewBuilder = new DataViewBuilder(
					TypeModeller.GetModelOf<TSource>(), null,
					viewConventions, domainDefinition,
					typeof(TSource)
					);

				_builtDelegate = builtDelegate;
				ViewDefinition = ViewBuilder.ViewDefinition;
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
		}
	}
}
