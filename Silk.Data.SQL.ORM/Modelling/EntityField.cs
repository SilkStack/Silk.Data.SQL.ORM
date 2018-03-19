using System;
using System.Collections.Generic;
using System.Linq;
using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Modelling
{
	public interface IEntityField : IField
	{
	}

	public interface IValueField : IEntityField
	{
		Column Column { get; }
	}

	public interface IEmbeddedObjectField : IEntityField
	{
		Column NullCheckColumn { get; }
		IEntityField[] EmbeddedFields { get; }
	}

	public interface ISingleRelatedObjectField : IEntityField
	{
		ProjectionModel RelatedObjectModel { get; }
		IValueField RelatedPrimaryKey { get; }
		Column LocalColumn { get; }
	}

	public interface IModelBuildFinalizerField
	{
		void FinalizeModelBuild(Schema.Schema finalizingSchema);
	}

	public class ValueField<T> : FieldBase<T>, IValueField
	{
		public Column Column { get; }

		public ValueField(string fieldName, bool canRead, bool canWrite, bool isEnumerable, Type elementType,
			Column column) :
			base(fieldName, canRead, canWrite, isEnumerable, elementType)
		{
			Column = column;
		}
	}

	public class EmbeddedObjectField<T> : FieldBase<T>, IEmbeddedObjectField, IModelBuildFinalizerField
	{
		public Column NullCheckColumn { get; }
		public IEntityField[] EmbeddedFields { get; }

		public EmbeddedObjectField(string fieldName, bool canRead, bool canWrite, bool isEnumerable, Type elementType,
			IEnumerable<IEntityField> embeddedFields, Column nullCheckColumn) :
			base(fieldName, canRead, canWrite, isEnumerable, elementType)
		{
			EmbeddedFields = embeddedFields.ToArray();
			NullCheckColumn = nullCheckColumn;
		}

		public void FinalizeModelBuild(Schema.Schema finalizingSchema)
		{
			foreach (var finalizerField in EmbeddedFields.OfType<IModelBuildFinalizerField>())
				finalizerField.FinalizeModelBuild(finalizingSchema);
		}
	}

	public class SingleRelatedObjectField<T> : FieldBase<T>, ISingleRelatedObjectField, IModelBuildFinalizerField
	{
		public ProjectionModel RelatedObjectModel { get; private set; }
		public IValueField RelatedPrimaryKey { get; }
		public Column LocalColumn { get; }

		public SingleRelatedObjectField(string fieldName, bool canRead, bool canWrite, bool isEnumerable, Type elementType,
			ProjectionModel relatedObjectModel, IValueField relatedPrimaryKey, Column localColumn) :
			base(fieldName, canRead, canWrite, isEnumerable, elementType)
		{
			RelatedObjectModel = relatedObjectModel;
			RelatedPrimaryKey = relatedPrimaryKey;
			LocalColumn = localColumn;
		}

		public void FinalizeModelBuild(Schema.Schema finalizingSchema)
		{
			if (RelatedObjectModel != null)
				return;
			RelatedObjectModel = finalizingSchema.EntityModels.FirstOrDefault(
				entityModel => entityModel.Fields.Contains(RelatedPrimaryKey)
				);
		}
	}

	public class ManyRelatedObjectField : FieldBase<ManyRelatedObjectField>, IEntityField
	{
		public SqlDataType SqlDataType => throw new NotImplementedException();
		public string SqlFieldName => throw new NotImplementedException();

		public ManyRelatedObjectField(string fieldName, bool canRead, bool canWrite, bool isEnumerable, Type elementType) :
			base(fieldName, canRead, canWrite, isEnumerable, elementType)
		{
		}
	}
}
