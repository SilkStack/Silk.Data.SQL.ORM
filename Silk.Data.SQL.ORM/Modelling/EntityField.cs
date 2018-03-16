using System;
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
		public ProjectionModel RelatedObjectModel { get; }
		public ValueField RelatedPrimaryKey { get; }
		public SqlDataType SqlDataType => throw new NotImplementedException();
		public string SqlFieldName => throw new NotImplementedException();

		public SingleRelatedObjectField(string fieldName, bool canRead, bool canWrite, bool isEnumerable, Type elementType) :
			base(fieldName, canRead, canWrite, isEnumerable, elementType)
		{
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
