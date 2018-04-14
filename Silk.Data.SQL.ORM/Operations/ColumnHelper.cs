using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silk.Data.SQL.ORM.Operations
{
	internal abstract class ColumnHelper
	{
		public string ColumnName { get; }

		public ColumnHelper(string columnName)
		{
			ColumnName = columnName;
		}

		public abstract QueryExpression GetColumnExpression(IModelReadWriter modelReadWriter);
	}

	internal class ColumnValueReader<T> : ColumnHelper
	{
		private readonly string[] _fieldPath;

		public ColumnValueReader(string columnName, string[] fieldPath) : base(columnName)
		{
			_fieldPath = fieldPath;
		}

		public override QueryExpression GetColumnExpression(IModelReadWriter modelReadWriter)
		{
			return QueryExpression.Value(
				modelReadWriter.ReadField<T>(_fieldPath, 0)
				);
		}
	}

	internal class ClientGeneratedValue<T> : ColumnHelper
	{
		private readonly string[] _fieldPath;

		public ClientGeneratedValue(string columnName, string[] fieldPath) : base(columnName)
		{
			if (typeof(T) != typeof(Guid))
				throw new InvalidOperationException($"Non-supported generated value type: {typeof(T).FullName}.");
			_fieldPath = fieldPath;
		}

		public override QueryExpression GetColumnExpression(IModelReadWriter modelReadWriter)
		{
			if (_fieldPath == null)
				return QueryExpression.Value(Guid.NewGuid());

			var value = modelReadWriter.ReadField<T>(_fieldPath, 0);
			if (!EqualityComparer<T>.Default.Equals(value, default(T)))
				return QueryExpression.Value(value);

			var newId = Guid.NewGuid();
			modelReadWriter.WriteField<Guid>(_fieldPath, 0, newId);
			return QueryExpression.Value(newId);
		}
	}

	internal abstract class ServerGeneratedValue : ColumnHelper
	{
		public ServerGeneratedValue(string columnName) : base(columnName)
		{
		}

		public abstract bool HasValue(IModelReadWriter modelReadWriter);

		public abstract void ReadResultValue(QueryResult queryResult, IModelReadWriter modelReadWriter);
	}

	internal class ServerGeneratedValue<T> : ServerGeneratedValue
	{
		private readonly string[] _fieldPath;

		public ServerGeneratedValue(string columnName, string[] fieldPath) : base(columnName)
		{
			_fieldPath = fieldPath;
		}

		public override QueryExpression GetColumnExpression(IModelReadWriter modelReadWriter)
		{
			if (_fieldPath == null)
				return QueryExpression.Value(null);

			var value = modelReadWriter.ReadField<T>(_fieldPath, 0);
			if (!EqualityComparer<T>.Default.Equals(value, default(T)))
				return QueryExpression.Value(value);

			return QueryExpression.Value(null);
		}

		public override bool HasValue(IModelReadWriter modelReadWriter)
		{
			var value = modelReadWriter.ReadField<T>(_fieldPath, 0);
			return !EqualityComparer<T>.Default.Equals(value, default(T));
		}

		public override void ReadResultValue(QueryResult queryResult, IModelReadWriter modelReadWriter)
		{
			if (_fieldPath == null)
				return;

			var value = modelReadWriter.ReadField<T>(_fieldPath, 0);
			if (!EqualityComparer<T>.Default.Equals(value, default(T)))
				return;

			var ord = queryResult.GetOrdinal(ColumnName);
			if (queryResult.IsDBNull(ord))
				return;

			if (typeof(int) == typeof(T))
				modelReadWriter.WriteField<int>(_fieldPath, 0, queryResult.GetInt32(ord));
			else if (typeof(short) == typeof(T))
				modelReadWriter.WriteField<short>(_fieldPath, 0, queryResult.GetInt16(ord));
			else if (typeof(long) == typeof(T))
				modelReadWriter.WriteField<long>(_fieldPath, 0, queryResult.GetInt64(ord));
		}
	}

	internal abstract class MultipleRelationshipReader : ColumnHelper
	{
		protected readonly ColumnHelper _valueReader;
		protected readonly string[] _path;

		public IManyRelatedObjectField Field { get; }

		public MultipleRelationshipReader(IManyRelatedObjectField field, ColumnHelper valueReader, string[] path) :
			base(null)
		{
			Field = field;
			_valueReader = valueReader;
			_path = path;
		}

		public abstract IEnumerable<QueryExpression> GetForeignKeyExpressions(IModelReadWriter modelReadWriter);

		public override QueryExpression GetColumnExpression(IModelReadWriter modelReadWriter)
		{
			throw new NotImplementedException();
		}
	}

	internal class MultipleRelationshipReader<TEnum,T> : MultipleRelationshipReader
		where TEnum : IEnumerable<T>
	{
		public MultipleRelationshipReader(IManyRelatedObjectField field, ColumnHelper valueReader, string[] path) :
			base(field, valueReader, path)
		{
		}

		public override IEnumerable<QueryExpression> GetForeignKeyExpressions(IModelReadWriter modelReadWriter)
		{
			var enumerable = modelReadWriter.ReadField<TEnum>(_path, 0);
			if (enumerable != null)
			{
				foreach (var item in enumerable)
				{
					var readWriter = new ObjectReadWriter(item, TypeModel.GetModelOf<T>(), typeof(T));
					yield return _valueReader.GetColumnExpression(readWriter);
				}
			}
		}
	}

	internal class ColumnHelperTransformer : IModelTransformer
	{
		private readonly ProjectionModel _projectionModel;
		private readonly Stack<string> _fieldStack = new Stack<string>();

		public List<ColumnHelper> Current { get; } = new List<ColumnHelper>();

		public ColumnHelperTransformer(ProjectionModel projectionModel)
		{
			_projectionModel = projectionModel;
		}

		private bool FieldIsOnModel(string[] path)
		{
			var fields = _projectionModel.Fields;
			foreach (var segment in path)
			{
				var field = fields.FirstOrDefault(q => q.FieldName == segment);
				if (field == null)
					return false;
				switch (field)
				{
					case IValueField valueField:
						fields = new IEntityField[0];
						break;
					case ISingleRelatedObjectField singleRelatedObjectField:
						fields = singleRelatedObjectField.RelatedObjectProjection.Fields;
						break;
					case IManyRelatedObjectField manyRelatedObjectField:
						fields = manyRelatedObjectField.RelatedObjectProjection.Fields;
						break;
					case IEmbeddedObjectField embeddedObjectField:
						fields = embeddedObjectField.EmbeddedFields;
						break;
				}
			}
			return true;
		}

		public void VisitField<T>(IField<T> field)
		{
			_fieldStack.Push(field.FieldName);

			switch (field)
			{
				case IProjectedValueField projectedValueField:
					{
						var column = projectedValueField.Column;
						var fieldPath = _fieldStack.Reverse().ToArray();
						if (!FieldIsOnModel(fieldPath))
							fieldPath = null;
						if (fieldPath != null)
							fieldPath = projectedValueField.Path;
						if (column.IsClientGenerated)
							Current.Add(new ClientGeneratedValue<T>(projectedValueField.Column.ColumnName, fieldPath));
						else if (column.IsServerGenerated)
							Current.Add(new ServerGeneratedValue<T>(projectedValueField.Column.ColumnName, fieldPath));
						else
							Current.Add(new ColumnValueReader<T>(projectedValueField.Column.ColumnName, fieldPath));
					}
					break;
				case IValueField valueField:
					{
						var column = valueField.Column;
						var fieldPath = _fieldStack.Reverse().ToArray();
						if (!FieldIsOnModel(fieldPath))
							fieldPath = null;
						if (column.IsClientGenerated)
							Current.Add(new ClientGeneratedValue<T>(valueField.Column.ColumnName, fieldPath));
						else if (column.IsServerGenerated)
							Current.Add(new ServerGeneratedValue<T>(valueField.Column.ColumnName, fieldPath));
						else
							Current.Add(new ColumnValueReader<T>(valueField.Column.ColumnName, fieldPath));
					}
					break;
				case ISingleRelatedObjectField singleRelatedObjectField:
					{
						var visitor = new SingleObjectFieldVisitor(this, _fieldStack, singleRelatedObjectField);
						singleRelatedObjectField.RelatedPrimaryKey.Transform(visitor);
					}
					break;
				case IEmbeddedObjectField embeddedObjectField:
					{
						foreach (var embeddedField in embeddedObjectField.EmbeddedFields)
						{
							embeddedField.Transform(this);
						}
					}
					break;
				case IManyRelatedObjectField manyRelatedObjectField:
					{
						var primaryKeyVisitor = new ManyObjectFieldVisitor(_fieldStack);
						manyRelatedObjectField.RelatedPrimaryKey.Transform(primaryKeyVisitor);
						var remotePrimaryKeyReader = primaryKeyVisitor.ColumnValueReader;
						if (remotePrimaryKeyReader != null)
						{
							var type = typeof(MultipleRelationshipReader<,>)
								.MakeGenericType(manyRelatedObjectField.FieldType, manyRelatedObjectField.ElementType);
							Current.Add(
								Activator.CreateInstance(type, manyRelatedObjectField, remotePrimaryKeyReader, _fieldStack.Reverse().ToArray()) as MultipleRelationshipReader
								);
						}
					}
					break;
			}

			_fieldStack.Pop();
		}

		public void VisitModel<TField>(IModel<TField> model) where TField : IField
		{
			throw new System.NotImplementedException();
		}

		private class ManyObjectFieldVisitor : IModelTransformer
		{
			private readonly Stack<string> _fieldStack;

			public ColumnHelper ColumnValueReader { get; private set; }

			public ManyObjectFieldVisitor(Stack<string> fieldStack)
			{
				_fieldStack = fieldStack;
			}

			public void VisitField<T>(IField<T> field)
			{
				switch (field)
				{
					case IProjectedValueField projectedValueField:
						{
							var column = projectedValueField.Column;
							var fieldPath = projectedValueField.Path;
							ColumnValueReader = new ColumnValueReader<T>(projectedValueField.Column.ColumnName, fieldPath);
						}
						break;
					case IValueField valueField:
						{
							var column = valueField.Column;
							var fieldPath = new[] { valueField.FieldName };
							ColumnValueReader = new ColumnValueReader<T>(valueField.Column.ColumnName, fieldPath);
						}
						break;
				}
			}

			public void VisitModel<TField>(IModel<TField> model) where TField : IField
			{
				throw new NotImplementedException();
			}
		}

		private class SingleObjectFieldVisitor : IModelTransformer
		{
			private readonly ColumnHelperTransformer _parent;
			private readonly Stack<string> _fieldStack;
			private readonly ISingleRelatedObjectField _field;

			public SingleObjectFieldVisitor(ColumnHelperTransformer parent, Stack<string> fieldStack,
				ISingleRelatedObjectField singleRelatedObjectField)
			{
				_parent = parent;
				_fieldStack = fieldStack;
				_field = singleRelatedObjectField;
			}

			public void VisitField<T>(IField<T> field)
			{
				if (!(field is IValueField valueField))
					return;

				var projectedField = field as IProjectedValueField;

				var column = valueField.Column;
				var fieldPath = _fieldStack.Reverse().Concat(new[] { valueField.FieldName }).ToArray();
				if (!_parent.FieldIsOnModel(fieldPath))
					fieldPath = null;
				if (fieldPath != null && projectedField != null)
					fieldPath = projectedField.Path;
				_parent.Current.Add(new ColumnValueReader<T>(_field.LocalColumn.ColumnName, fieldPath));
			}

			public void VisitModel<TField>(IModel<TField> model) where TField : IField
			{
				throw new NotImplementedException();
			}
		}
	}
}
