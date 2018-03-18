using System;
using System.Linq;
using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Modelling
{
	public interface IEntityField : IField
	{
	}

	public class ValueField : FieldBase<ValueField>, IEntityField
	{
		public Column Column { get; }

		public ValueField(string fieldName, bool canRead, bool canWrite, bool isEnumerable, Type elementType,
			Column column) :
			base(fieldName, canRead, canWrite, isEnumerable, elementType)
		{
			Column = column;
		}
	}

	public class SingleRelatedObjectField : FieldBase<SingleRelatedObjectField>, IEntityField
	{
		public ProjectionModel RelatedObjectModel { get; private set; }
		public ValueField RelatedPrimaryKey { get; }
		public Column LocalColumn { get; }

		public SingleRelatedObjectField(string fieldName, bool canRead, bool canWrite, bool isEnumerable, Type elementType,
			ProjectionModel relatedObjectModel, ValueField relatedPrimaryKey, Column localColumn) :
			base(fieldName, canRead, canWrite, isEnumerable, elementType)
		{
			RelatedObjectModel = relatedObjectModel;
			RelatedPrimaryKey = relatedPrimaryKey;
			LocalColumn = localColumn;
		}

		internal void UpdateRelatedObjectModel(EntityModelCollection entityModels)
		{
			if (RelatedObjectModel != null)
				return;
			RelatedObjectModel = entityModels.FirstOrDefault(
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
