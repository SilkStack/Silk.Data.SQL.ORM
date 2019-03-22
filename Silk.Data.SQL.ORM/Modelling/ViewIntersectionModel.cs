using System;
using System.Collections.Generic;
using System.Linq;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.GenericDispatch;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class ViewIntersectionModel : IModel<ViewIntersectionField>
	{
		private readonly ViewIntersectionField[] _fields;

		public TypeModel OriginTypeModel { get; }

		public IReadOnlyList<ViewIntersectionField> Fields => _fields;

		IReadOnlyList<IField> IModel.Fields => _fields;

		public ViewIntersectionModel(
			TypeModel originTypeModel,
			IEnumerable<ViewIntersectionField> fields
			)
		{
			OriginTypeModel = originTypeModel;
			_fields = fields.ToArray();
			foreach (var field in _fields)
				field.Parent = this;
		}

		internal void Replace(ViewIntersectionField old, ViewIntersectionField @new)
		{
			var index = -1;
			for (var i = 0; i < _fields.Length; i++)
			{
				if (_fields[i] == old)
				{
					index = i;
					break;
				}
			}
			if (index == -1)
				throw new InvalidOperationException("Field not found.");
			_fields[index] = @new;
		}


		public void Dispatch(IModelGenericExecutor executor)
			=> executor.Execute<ViewIntersectionModel, ViewIntersectionField>(this);

		public IEnumerable<ViewIntersectionField> GetPathFields(IFieldPath<ViewIntersectionField> fieldPath)
		{
			if (fieldPath.FinalField == null)
				return Fields;
			return fieldPath.FinalField.SelfModel.Fields;
		}

		internal static ViewIntersectionModel FromTypeModel(TypeModel typeModel)
		{
			var builder = new FieldBuilder(typeModel);
			return new ViewIntersectionModel(
				typeModel,
				typeModel.Fields.Where(q => !q.IsEnumerableType)
					.Select(q => BuildField(q, builder))
				);
		}

		private static ViewIntersectionField BuildField(PropertyInfoField sourceField, FieldBuilder builder)
		{
			sourceField.Dispatch(builder);
			return builder.Field;
		}

		private class FieldBuilder : IFieldGenericExecutor
		{
			private readonly TypeModel _typeModel;

			public ViewIntersectionField Field { get; private set; }

			public FieldBuilder(TypeModel typeModel)
			{
				_typeModel = typeModel;
			}

			void IFieldGenericExecutor.Execute<TField, TData>(IField field)
			{
				Field = new ViewIntersectionField<TData>(
					field.FieldName, field.CanRead, field.CanWrite, field.FieldDataType, _typeModel, field as PropertyInfoField
					);
			}
		}
	}

	public abstract class ViewIntersectionField : IField
	{
		protected ViewIntersectionField(
			string fieldName, bool canRead, bool canWrite, Type fieldDataType, TypeModel declaringTypeModel,
			PropertyInfoField originPropertyField
			)
		{
			FieldName = fieldName;
			CanRead = canRead;
			CanWrite = canWrite;
			FieldDataType = fieldDataType;
			DeclaringTypeModel = declaringTypeModel;
			OriginPropertyField = originPropertyField;

			SelfModel = ViewIntersectionModel.FromTypeModel(
				TypeModel.GetModelOf(fieldDataType)
				);
		}

		public string FieldName { get; }

		public bool CanRead { get; }

		public bool CanWrite { get; }

		public bool IsEnumerableType => false;

		public Type FieldDataType { get; }

		public Type FieldElementType => null;

		public TypeModel DeclaringTypeModel { get; }

		public PropertyInfoField OriginPropertyField { get; }

		public ViewIntersectionModel Parent { get; internal set; }

		public ViewIntersectionModel SelfModel { get; }

		internal void Replace(ViewIntersectionField newField)
		{
			Parent.Replace(this, newField);
		}

		public abstract void Dispatch(IFieldGenericExecutor executor);
	}

	public class ViewIntersectionField<TData> : ViewIntersectionField
	{
		public ViewIntersectionField(string fieldName, bool canRead, bool canWrite, Type fieldDataType, TypeModel declaringTypeModel, PropertyInfoField originPropertyField) : base(fieldName, canRead, canWrite, fieldDataType, declaringTypeModel, originPropertyField)
		{
		}

		public override void Dispatch(IFieldGenericExecutor executor)
			=> executor.Execute<ViewIntersectionField, TData>(this);
	}

	public class ConvertedViewIntersectionField<TSourceData, TDestinationData> : ViewIntersectionField
	{
		private readonly IFieldPath<ViewIntersectionModel, ViewIntersectionField> _path;
		private readonly TryConvertDelegate<TSourceData, TDestinationData> _tryConvertDelegate;

		public ConvertedViewIntersectionField(
			string fieldName, bool canRead, bool canWrite, TypeModel declaringTypeModel,
			PropertyInfoField originPropertyField, IFieldPath<ViewIntersectionModel, ViewIntersectionField> path,
			TryConvertDelegate<TSourceData, TDestinationData> tryConvertDelegate
			) :
			base(
				fieldName, canRead, canWrite, typeof(TDestinationData), declaringTypeModel, originPropertyField
				)
		{
			_path = path;
			_tryConvertDelegate = tryConvertDelegate;
		}

		public override void Dispatch(IFieldGenericExecutor executor)
			=> executor.Execute<ViewIntersectionField, TDestinationData>(this);
	}
}
