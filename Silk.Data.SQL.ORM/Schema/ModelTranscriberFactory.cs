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
			IIntersection<TypeModel, PropertyInfoField, EntityModel, EntityField> typeToModelIntersection
			)
			where TEntity : class
			where TView : class
		{
			var fieldWriterBuilder = new FieldWriterBuilder<TView>();
			var fieldWriters = new List<IntersectedFieldWriter<TView>>();
			foreach (var intersectedFields in typeToModelIntersection.IntersectedFields
				.Where(q => q.RightField.IsEntityLocalField))
			{
				if (IsFieldBound(fieldWriters, intersectedFields))
					continue;

				intersectedFields.Dispatch(fieldWriterBuilder);
				fieldWriters.Add(fieldWriterBuilder.FieldWriter);
			}

			return new ModelTranscriber<TView>(typeToModelIntersection, fieldWriters);
		}

		private static bool IsFieldBound<TView>(
			List<IntersectedFieldWriter<TView>> fieldWriters,
			IntersectedFields<TypeModel, PropertyInfoField, EntityModel, EntityField> intersectedFields
			)
			where TView : class
			=> fieldWriters.Any(writer => writer.To.Column.Name == intersectedFields.RightField.Column.Name);

		private class FieldWriterBuilder<TView> : IIntersectedFieldsGenericExecutor
			where TView : class
		{
			public IntersectedFieldWriter<TView> FieldWriter { get; private set; }

			void IIntersectedFieldsGenericExecutor.Execute<TLeftModel, TLeftField, TRightModel, TRightField, TLeftData, TRightData>(
				IntersectedFields<TLeftModel, TLeftField, TRightModel, TRightField, TLeftData, TRightData> intersectedFields
				)
			{
				FieldWriter = new IntersectedFieldWriter<TView, TLeftData, TRightData>(
					intersectedFields as IntersectedFields<TypeModel, PropertyInfoField, EntityModel, EntityField, TLeftData, TRightData>
					);
			}
		}

		private class ModelTranscriber<TView> : IModelTranscriber<TView>
			where TView : class
		{
			public IIntersection<TypeModel, PropertyInfoField, EntityModel, EntityField> TypeModelToEntityModelIntersection { get; }

			public IReadOnlyList<IntersectedFieldWriter<TView>> FieldWriters { get; }

			public ModelTranscriber(
				IIntersection<TypeModel, PropertyInfoField, EntityModel, EntityField> typeModelToEntityModelIntersection,
				IReadOnlyList<IntersectedFieldWriter<TView>> fieldWriters
				)
			{
				TypeModelToEntityModelIntersection = typeModelToEntityModelIntersection;
				FieldWriters = fieldWriters;
			}
		}
	}
}
