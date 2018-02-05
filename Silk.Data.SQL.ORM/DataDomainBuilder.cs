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
		//private static ViewConvention[] _defaultViewConventions
		//	= new ViewConvention[]
		//	{
		//		new CleanModelNameConvention(),
		//		new CopyPrimitiveTypesConvention(),
		//		new IdIsPrimaryKeyConvention(),
		//		new ManyToOneConvention(),
		//		new ManyToManyConvention(),
		//		new FlattenPocosConvention()
		//	};

		private static ISchemaConvention[] _defaultSchemaConventions
			= new ISchemaConvention[]
			{
				new SQLTypesConvention(),
				new CleanModelNameConvention()
			};

		private static IProjectionConvention[] _defaultProjectionConventions
			= new IProjectionConvention[0];

		private readonly Dictionary<Type, EntityTypeHelper> _entityTypes = new Dictionary<Type, EntityTypeHelper>();

		public ModelCustomizer<TSource> AddEntityType<TSource>()
			where TSource : new()
		{
			//  todo: decide what is appropriate if the same type is added twice:
			//    throw exception?
			//    return the customizer for the already present model?

			var helper = new EntityTypeHelper<TSource>();
			_entityTypes[typeof(TSource)] = helper;
			return helper.Customizer as ModelCustomizer<TSource>;
		}

		private Dictionary<TypedModel,ModelOpinions> GetModelOpinions()
		{
			return _entityTypes.ToDictionary(
				q => q.Value.Model,
				q => q.Value.Customizer.GetModelOpinions()
				);
		}

		private SchemaDefinition BuildSchemaDefinition(ISchemaConvention[] schemaConventions, SchemaBuilderWithAlterReset schemaBuilder)
		{
			while (true)
			{
				schemaBuilder.ResetWasAlteredFlag();

				foreach (var entityModel in schemaBuilder.EntityModels)
				{
					foreach (var convention in schemaConventions)
					{
						convention.VisitModel(entityModel, schemaBuilder);
					}
				}

				if (!schemaBuilder.WasAltered)
					break;
			}
			return schemaBuilder.BuildDefinition();
		}

		private NewModelling.DataField[] CreateFields(EntityDefinition entityDefinition)
		{
			var ret = new NewModelling.DataField[entityDefinition.Fields.Count];
			for (var i = 0; i < ret.Length; i++)
			{
				var definition = entityDefinition.Fields[i];
				ret[i] = new NewModelling.DataField(definition.Name, definition.ClrType, definition.SqlDataType,
					definition.Binding, definition.IsPrimaryKey, definition.AutoGenerate, definition.IsIndex, definition.IsUnique);
			}
			return ret;
		}

		public DataDomain Build(ISchemaConvention[] schemaConventions = null, IProjectionConvention[] projectionConventions = null)
		{
			if (schemaConventions == null)
				schemaConventions = _defaultSchemaConventions;
			if (projectionConventions == null)
				projectionConventions = _defaultProjectionConventions;

			var schemaBuilder = new SchemaBuilderWithAlterReset(GetModelOpinions());
			var schemaDefinition = BuildSchemaDefinition(schemaConventions, schemaBuilder);

			foreach (var entityHelper in _entityTypes.Values)
			{
				var entityDefinition = schemaDefinition.Entities.FirstOrDefault(q => q.EntityModel == entityHelper.Model);
				if (entityDefinition == null)
					continue;

				var dataFields = CreateFields(entityDefinition);
				var entityTable = new NewModelling.Table(
					entityDefinition.TableName, true, dataFields, entityDefinition.EntityModel.DataType
					);

				entityHelper.EntityTable = entityTable;
				entityHelper.Tables.Add(entityTable);
				entityHelper.EntitySchema.Fields = dataFields;
			}

			return new DataDomain(_entityTypes.Values.Select(q => q.EntitySchema), projectionConventions);

			//var viewDefinitionFieldCounts = new Dictionary<ViewDefinition, int>();
			//var currentViewDefinitionFieldCounts = new Dictionary<ViewDefinition, int>();
			//DataDomain builtDomain = null;
			//var lazyDomainAccessor = new Lazy<DataDomain>(() => builtDomain);

			//foreach (var entityModelBuilder in _entityModelBuilders)
			//{
			//	viewDefinitionFieldCounts.Add(entityModelBuilder.ViewDefinition, 0);
			//}

			//var fieldsChanged = true;
			//while (fieldsChanged)
			//{
			//	fieldsChanged = false;

			//	foreach (var entityModelBuilder in _entityModelBuilders)
			//	{
			//		entityModelBuilder.ViewBuilder.ProcessModel(entityModelBuilder.ViewDefinition.TargetModel);

			//		currentViewDefinitionFieldCounts[entityModelBuilder.ViewDefinition] = entityModelBuilder.ViewDefinition.FieldDefinitions.Count;
			//		if (currentViewDefinitionFieldCounts[entityModelBuilder.ViewDefinition] !=
			//			viewDefinitionFieldCounts[entityModelBuilder.ViewDefinition])
			//			fieldsChanged = true;
			//		viewDefinitionFieldCounts[entityModelBuilder.ViewDefinition] = entityModelBuilder.ViewDefinition.FieldDefinitions.Count;

			//		entityModelBuilder.ViewBuilder.IsFirstPass = false;
			//	}
			//}
			//foreach (var entityModelBuilder in _entityModelBuilders)
			//{
			//	entityModelBuilder.ViewBuilder.FinalizeModel();
			//}

			//foreach (var entityModelBuilder in _entityModelBuilders)
			//{
			//	entityModelBuilder.CustomizeModel();
			//}

			//builtDomain = DataDomain.CreateFromDefinition(_domainDefinition, _viewConventions);

			//foreach (var entityModelBuilder in _entityModelBuilders)
			//{
			//	entityModelBuilder.CallBuiltDelegate(builtDomain);
			//}

			//return builtDomain;
		}

		private class SchemaBuilderWithAlterReset : SchemaBuilder
		{
			public SchemaBuilderWithAlterReset(Dictionary<TypedModel, ModelOpinions> modelOpinions)
				: base(modelOpinions)
			{
			}

			public void ResetWasAlteredFlag()
			{
				WasAltered = false;
			}
		}

		private abstract class EntityTypeHelper
		{
			public TypedModel Model { get; }
			public ModelCustomizer Customizer { get; }
			public NewModelling.EntitySchema EntitySchema { get; }
			public List<NewModelling.Table> Tables { get; } = new List<NewModelling.Table>();
			public NewModelling.Table EntityTable { get; set; }

			public EntityTypeHelper(TypedModel model,
				ModelCustomizer customizer, NewModelling.EntitySchema entitySchema)
			{
				Model = model;
				Customizer = customizer;
				EntitySchema = entitySchema;
			}
		}

		private class EntityTypeHelper<T> : EntityTypeHelper
			where T : new()
		{
			public EntityTypeHelper()
				: base(TypeModeller.GetModelOf<T>(), new ModelCustomizer<T>(TypeModeller.GetModelOf<T>()),
					  new NewModelling.EntitySchema<T>(TypeModeller.GetModelOf<T>()))
			{
			}
		}

		//private abstract class EntityModelBuilder
		//{
		//	public abstract DataViewBuilder ViewBuilder { get; }
		//	public abstract Type ModelType { get; }
		//	public abstract Type ProjectionType { get; }
		//	public ViewDefinition ViewDefinition { get; protected set; }
		//	public DomainDefinition DomainDefinition { get; protected set; }

		//	public abstract void CallBuiltDelegate(DataDomain dataDomain);
		//	public abstract void CustomizeModel();
		//}

		//private class EntityModelBuilder<TSource> : EntityModelBuilder
		//	where TSource : new()
		//{
		//	private readonly Action<EntityModel<TSource>> _builtDelegate;

		//	public override Type ModelType => typeof(TSource);
		//	public override Type ProjectionType => null;
		//	public override DataViewBuilder ViewBuilder { get; }
		//	private Action<ModelCustomizer<TSource>> _modelCustomizerFunc;

		//	public EntityModelBuilder(Action<EntityModel<TSource>> builtDelegate,
		//		ViewConvention[] viewConventions, DomainDefinition domainDefinition,
		//		Action<ModelCustomizer<TSource>> modelCustomizerFunc)
		//	{
		//		ViewBuilder = new DataViewBuilder(
		//			TypeModeller.GetModelOf<TSource>(), null,
		//			viewConventions, domainDefinition,
		//			typeof(TSource)
		//			);

		//		_builtDelegate = builtDelegate;
		//		ViewDefinition = ViewBuilder.ViewDefinition;
		//		_modelCustomizerFunc = modelCustomizerFunc;
		//	}

		//	public override void CallBuiltDelegate(DataDomain dataDomain)
		//	{
		//		_builtDelegate?.Invoke(dataDomain.GetEntityModel<TSource>());
		//	}

		//	public override void CustomizeModel()
		//	{
		//		_modelCustomizerFunc?.Invoke(new ModelCustomizer<TSource>(null));
		//	}
		//}

		//private class EntityModelBuilder<TSource,TView> : EntityModelBuilder
		//	where TSource : new()
		//	where TView : new()
		//{
		//	private readonly Action<EntityModel<TSource, TView>> _builtDelegate;

		//	public override Type ModelType => typeof(TSource);
		//	public override Type ProjectionType => typeof(TView);
		//	public override DataViewBuilder ViewBuilder { get; }

		//	public EntityModelBuilder(Action<EntityModel<TSource, TView>> builtDelegate,
		//		ViewConvention[] viewConventions, DomainDefinition domainDefinition)
		//	{
		//		ViewBuilder = new DataViewBuilder(
		//			TypeModeller.GetModelOf<TSource>(), 
		//			TypeModeller.GetModelOf<TView>(),
		//			viewConventions, domainDefinition,
		//			typeof(TSource), typeof(TView)
		//			);

		//		_builtDelegate = builtDelegate;
		//		ViewDefinition = ViewBuilder.ViewDefinition;
		//	}

		//	public override void CallBuiltDelegate(DataDomain dataDomain)
		//	{
		//		if (_builtDelegate != null)
		//			_builtDelegate(dataDomain.GetEntityModel<TSource>() as EntityModel<TSource, TView>);
		//	}

		//	public override void CustomizeModel()
		//	{
		//	}
		//}
	}
}
