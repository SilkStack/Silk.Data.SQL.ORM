using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.GenericDispatch;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public class ModelTranscriberFactory
	{
		public static IModelTranscriber<TView> Create<TEntity, TView>(
			IIntersection<TypeModel, PropertyInfoField, EntityModel, EntityField> typeToModelIntersection,
			IIntersection<EntityModel, EntityField, TypeModel, PropertyInfoField> entityToTypeIntersection
			)
			where TEntity : class
			where TView : class
		{
			var entityHelperBuilder = new EntityHelperBuilder<TView>();
			var entityHelpers = new List<EntityModelHelper<TView>>();
			var typeHelperBuilder = new TypeHelperBuilder<TView>();
			var typeHelpers = new List<TypeModelHelper<TView>>();

			foreach (var intersectedFields in typeToModelIntersection.IntersectedFields
				.Where(q => q.RightField.IsEntityLocalField))
			{
				if (IsFieldBound(entityHelpers, intersectedFields))
					continue;

				intersectedFields.Dispatch(entityHelperBuilder);
				entityHelpers.Add(entityHelperBuilder.EntityHelper);
			}

			foreach (var intersectedFields in entityToTypeIntersection.IntersectedFields)
			{
				if (IsFieldBound(typeHelpers, intersectedFields))
					continue;

				intersectedFields.Dispatch(typeHelperBuilder);
				typeHelpers.Add(typeHelperBuilder.TypeHelper);
			}

			return new ModelTranscriber<TView>(
				typeToModelIntersection, entityHelpers,
				entityToTypeIntersection, typeHelpers
				);
		}

		private static bool IsFieldBound<TView>(
			List<EntityModelHelper<TView>> fieldWriters,
			IntersectedFields<TypeModel, PropertyInfoField, EntityModel, EntityField> intersectedFields
			)
			where TView : class
			=> fieldWriters.Any(writer => writer.To.Column.Name == intersectedFields.RightField.Column.Name);

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

		private class EntityHelperBuilder<TView> : IIntersectedFieldsGenericExecutor
			where TView : class
		{
			public EntityModelHelper<TView> EntityHelper { get; private set; }

			void IIntersectedFieldsGenericExecutor.Execute<TLeftModel, TLeftField, TRightModel, TRightField, TLeftData, TRightData>(
				IntersectedFields<TLeftModel, TLeftField, TRightModel, TRightField, TLeftData, TRightData> intersectedFields
				)
			{
				EntityHelper = new EntityModelHelper<TView, TLeftData, TRightData>(
					intersectedFields as IntersectedFields<TypeModel, PropertyInfoField, EntityModel, EntityField, TLeftData, TRightData>
					);
			}
		}

		private class ModelTranscriber<TView> : IModelTranscriber<TView>
			where TView : class
		{
			public IIntersection<TypeModel, PropertyInfoField, EntityModel, EntityField> TypeModelToEntityModelIntersection { get; }

			public IIntersection<EntityModel, EntityField, TypeModel, PropertyInfoField> EntityModelToTypeModelIntersection { get; }

			public IReadOnlyList<EntityModelHelper<TView>> EntityModelHelpers { get; }

			public IReadOnlyList<TypeModelHelper<TView>> TypeModelHelpers { get; }

			public ModelTranscriber(
				IIntersection<TypeModel, PropertyInfoField, EntityModel, EntityField> typeModelToEntityModelIntersection,
				IReadOnlyList<EntityModelHelper<TView>> entityModelHelpers,
				IIntersection<EntityModel, EntityField, TypeModel, PropertyInfoField> entityModelToTypeModelIntersection,
				IReadOnlyList<TypeModelHelper<TView>> typeModelHelpers
				)
			{
				TypeModelToEntityModelIntersection = typeModelToEntityModelIntersection;
				EntityModelHelpers = entityModelHelpers;
				EntityModelToTypeModelIntersection = entityModelToTypeModelIntersection;
				TypeModelHelpers = typeModelHelpers;
			}
		}
	}
}
