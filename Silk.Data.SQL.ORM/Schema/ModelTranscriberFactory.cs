using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.GenericDispatch;
using Silk.Data.Modelling.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public class ModelTranscriberFactory
	{
		public static IModelTranscriber<TView> Create<TEntity, TView>(
			IIntersectionAnalyzer<TypeModel, PropertyInfoField, EntityModel, EntityField> typeToModelAnalyzer,
			IIntersectionAnalyzer<EntityModel, EntityField, TypeModel, PropertyInfoField> entityToTypeAnalyzer,
			EntityModel<TEntity> entityModel
			)
			where TEntity : class
			where TView : class
		{
			var typeHelperBuilder = new TypeHelperBuilder<TView>();
			var typeHelpers = new List<TypeModelHelper<TView>>();

			var viewTypeModel = TypeModel.GetModelOf<TView>();
			var entityTransformers = BuildModelFieldTransformers(
				typeToModelAnalyzer, entityModel, viewTypeModel
				);
			var entityHelpers = BuildEntityHelpers(
				typeToModelAnalyzer, entityModel, viewTypeModel
				);

			var typeToModelIntersection = typeToModelAnalyzer.CreateIntersection(viewTypeModel, entityModel);
			var entityToTypeIntersection = entityToTypeAnalyzer.CreateIntersection(entityModel, viewTypeModel);

			foreach (var intersectedFields in entityToTypeIntersection.IntersectedFields)
			{
				if (IsFieldBound(typeHelpers, intersectedFields))
					continue;

				intersectedFields.Dispatch(typeHelperBuilder);
				typeHelpers.Add(typeHelperBuilder.TypeHelper);
			}

			return new ModelTranscriber<TView>(
				typeToModelIntersection, entityHelpers, entityTransformers,
				entityToTypeIntersection, typeHelpers
				);
		}

		private static List<EntityModelFieldTransformer<TView>> BuildModelFieldTransformers<TEntity, TView>(
			IIntersectionAnalyzer<TypeModel, PropertyInfoField, EntityModel, EntityField> typeToModelAnalyzer,
			EntityModel<TEntity> entityModel,
			TypeModel<TView> viewTypeModel)
			where TEntity : class
			where TView : class
		{
			var builder = new EntityFieldTransformerBuilder<TView>();
			var transformers = new List<EntityModelFieldTransformer<TView>>();
			var viewToModelIntersection = typeToModelAnalyzer.CreateIntersection(viewTypeModel, entityModel);
			var entityToModelIntersection = typeToModelAnalyzer.CreateIntersection(TypeModel.GetModelOf<TEntity>(), entityModel);

			foreach (var intersectedFields in viewToModelIntersection.IntersectedFields
					.Where(q => q.RightField.IsEntityLocalField))
			{
				if (intersectedFields.RightField is ObjectEntityField<TEntity>)
				{
					intersectedFields.Dispatch(builder);
					transformers.Add(builder.FieldTransformer);
				}
			}

			return transformers;
		}

		private static List<EntityModelHelper<TView>> BuildEntityHelpers<TEntity, TView>(
			IIntersectionAnalyzer<TypeModel, PropertyInfoField, EntityModel, EntityField> typeToModelAnalyzer,
			EntityModel<TEntity> entityModel,
			TypeModel<TView> viewTypeModel
			)
			where TEntity : class
			where TView : class
		{
			var entityHelperBuilder = new EntityHelperBuilder<TView>();
			var entityHelpers = new List<EntityModelHelper<TView>>();
			var viewToModelIntersection = typeToModelAnalyzer.CreateIntersection(viewTypeModel, entityModel);
			var entityToModelIntersection = typeToModelAnalyzer.CreateIntersection(TypeModel.GetModelOf<TEntity>(), entityModel);

			foreach (var intersectedFields in viewToModelIntersection.IntersectedFields
					.Where(q => q.RightField.IsEntityLocalField))
			{
				if (IsFieldBound(entityHelpers, intersectedFields))
					continue;

				if (intersectedFields.RightField is ValueEntityField<TEntity>)
				{
					intersectedFields.Dispatch(entityHelperBuilder);
					entityHelpers.Add(entityHelperBuilder.EntityHelper);
				}
				else if (intersectedFields.RightField is ObjectEntityField<TEntity>)
				{
					entityHelperBuilder.ObjectPath.Push(intersectedFields.LeftField.FieldName);
					CopyConvertedObjectFields(intersectedFields);
					entityHelperBuilder.ObjectPath.Pop();
				}
			}

			return entityHelpers;

			void CopyConvertedObjectFields(
				IntersectedFields<TypeModel, PropertyInfoField, EntityModel, EntityField> intersectedFields
				)
			{
				foreach (var objectField in intersectedFields.RightField.FieldDataTypeModel.Fields)
				{
					var entityIntersectionField = entityToModelIntersection.IntersectedFields
						.FirstOrDefault(q => q.LeftField.Property == objectField.Property);

					if (entityIntersectionField == null)
						continue;  //  todo: change to exception? this shouldn't occur
					if (IsFieldBound(entityHelpers, entityIntersectionField))
						continue;

					if (entityIntersectionField.RightField is ValueEntityField<TEntity>)
					{
						entityIntersectionField.Dispatch(entityHelperBuilder);
						entityHelpers.Add(entityHelperBuilder.EntityHelper);
					}
					else if (entityIntersectionField.RightField is ObjectEntityField<TEntity>)
					{
						entityHelperBuilder.ObjectPath.Push(intersectedFields.LeftField.FieldName);
						CopyConvertedObjectFields(entityIntersectionField);
						entityHelperBuilder.ObjectPath.Pop();
					}
				}
			}
		}

		private static bool IsFieldBound<TView>(
			List<EntityModelHelper<TView>> fieldWriters,
			IntersectedFields<TypeModel, PropertyInfoField, EntityModel, EntityField> intersectedFields
			)
			where TView : class
			=> fieldWriters.Any(writer => writer.ToPath.Fields.Select(q => q.FieldName).SequenceEqual(
				intersectedFields.RightPath.Fields.Select(q => q.FieldName)
				));

		private static bool IsFieldBound<TView>(
			List<TypeModelHelper<TView>> typeHelpers,
			IntersectedFields<EntityModel, EntityField, TypeModel, PropertyInfoField> intersectedFields
			)
			where TView : class
			=> typeHelpers.Any(helper => helper.ToPath.Fields.Select(q => q.FieldName).SequenceEqual(
				intersectedFields.RightPath.Fields.Select(q => q.FieldName)
				));

		private class TypeHelperBuilder<TView> : IIntersectedFieldsGenericExecutor
			where TView : class
		{
			public TypeModelHelper<TView> TypeHelper { get; private set; }

			void IIntersectedFieldsGenericExecutor.Execute<TLeftModel, TLeftField, TRightModel, TRightField, TLeftData, TRightData>(IntersectedFields<TLeftModel, TLeftField, TRightModel, TRightField, TLeftData, TRightData> intersectedFields)
			{
				TypeHelper = new TypeModelHelper<TView, TLeftData, TRightData>(
					intersectedFields as IntersectedFields<EntityModel, EntityField, TypeModel, PropertyInfoField, TLeftData, TRightData>
					);
			}
		}

		private class EntityFieldTransformerBuilder<TView> : IIntersectedFieldsGenericExecutor
			where TView : class
		{
			public EntityModelFieldTransformer<TView> FieldTransformer { get; private set; }

			void IIntersectedFieldsGenericExecutor.Execute<TLeftModel, TLeftField, TRightModel, TRightField, TLeftData, TRightData>(
				IntersectedFields<TLeftModel, TLeftField, TRightModel, TRightField, TLeftData, TRightData> intersectedFields
				)
			{
				var castFields = intersectedFields as IntersectedFields<TypeModel, PropertyInfoField, EntityModel, EntityField, TLeftData, TRightData>;
				FieldTransformer = new EntityModelFieldTransformer<TView, TLeftData, TRightData>(
					castFields.LeftPath,
					//  todo: make a proper path relative to the entity type here, this only works because test cases share the same paths
					castFields.LeftPath,
					castFields.GetConvertDelegate()
					);
			}
		}

		private class EntityHelperBuilder<TView> : IIntersectedFieldsGenericExecutor
			where TView : class
		{
			public EntityModelHelper<TView> EntityHelper { get; private set; }
			public Stack<string> ObjectPath { get; } = new Stack<string>();

			void IIntersectedFieldsGenericExecutor.Execute<TLeftModel, TLeftField, TRightModel, TRightField, TLeftData, TRightData>(
				IntersectedFields<TLeftModel, TLeftField, TRightModel, TRightField, TLeftData, TRightData> intersectedFields
				)
			{
				EntityHelper = new EntityModelHelper<TView, TLeftData, TRightData>(
					intersectedFields as IntersectedFields<TypeModel, PropertyInfoField, EntityModel, EntityField, TLeftData, TRightData>,
					string.Join(".", ObjectPath)
					);
			}
		}

		private class ModelTranscriber<TView> : IModelTranscriber<TView>
			where TView : class
		{
			public IIntersection<TypeModel, PropertyInfoField, EntityModel, EntityField> TypeModelToEntityModelIntersection { get; }

			public IIntersection<EntityModel, EntityField, TypeModel, PropertyInfoField> EntityModelToTypeModelIntersection { get; }

			public IReadOnlyList<EntityModelHelper<TView>> ObjectToSchemaHelpers { get; }

			public IReadOnlyList<TypeModelHelper<TView>> SchemaToTypeHelpers { get; }

			public IMapping<EntityModel, EntityField, TypeModel, PropertyInfoField> Mapping { get; }

			public IReadOnlyList<EntityModelFieldTransformer<TView>> EntityFieldTransformers { get; }

			public ModelTranscriber(
				IIntersection<TypeModel, PropertyInfoField, EntityModel, EntityField> typeModelToEntityModelIntersection,
				IReadOnlyList<EntityModelHelper<TView>> entityModelHelpers,
				IReadOnlyList<EntityModelFieldTransformer<TView>> entityFieldTransformers,
				IIntersection<EntityModel, EntityField, TypeModel, PropertyInfoField> entityModelToTypeModelIntersection,
				IReadOnlyList<TypeModelHelper<TView>> typeModelHelpers
				)
			{
				TypeModelToEntityModelIntersection = typeModelToEntityModelIntersection;
				ObjectToSchemaHelpers = entityModelHelpers;
				EntityModelToTypeModelIntersection = entityModelToTypeModelIntersection;
				SchemaToTypeHelpers = typeModelHelpers;
				EntityFieldTransformers = entityFieldTransformers;

				var mappingFactory = new DefaultMappingFactory<EntityModel, EntityField, TypeModel, PropertyInfoField>();
				Mapping = mappingFactory.CreateMapping(entityModelToTypeModelIntersection);
			}
		}
	}
}
