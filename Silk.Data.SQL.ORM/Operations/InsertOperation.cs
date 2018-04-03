using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Operations.Expressions;
using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Operations
{
	public class InsertOperation : DataOperation
	{
		public bool GeneratesValuesServerSide { get; private set; }

		private readonly QueryExpression _bulkInsertExpression;
		private readonly List<IndividualInsert> _individualInserts;
		private readonly ServerGeneratedValue[] _serverGeneratedValues;

		public override bool CanBeBatched => !GeneratesValuesServerSide;

		private InsertOperation(QueryExpression bulkInsertExpression,
			List<IndividualInsert> individualInserts)
		{
			GeneratesValuesServerSide = individualInserts.Count > 0;
			_bulkInsertExpression = bulkInsertExpression;
			_individualInserts = individualInserts;
		}

		public override QueryExpression GetQuery()
		{
			if (_individualInserts == null || _individualInserts.Count < 1)
				return _bulkInsertExpression;

			if (_bulkInsertExpression != null)
				return new CompositeQueryExpression(
					_individualInserts.Select(q => q.Query).Concat(new[] { _bulkInsertExpression }).ToArray()
					);
			return new CompositeQueryExpression(
				_individualInserts.Select(q => q.Query).ToArray()
				);
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			if (_individualInserts == null || _individualInserts.Count < 1)
				return;

			foreach (var individualInsert in _individualInserts)
			{
				queryResult.Read();
				individualInsert.MapOntoObject(queryResult);
				queryResult.NextResult();
			}
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			if (_individualInserts == null || _individualInserts.Count < 1)
				return;

			foreach (var individualInsert in _individualInserts)
			{
				await queryResult.ReadAsync();
				individualInsert.MapOntoObject(queryResult);
				await queryResult.NextResultAsync();
			}
		}

		public static InsertOperation Create<TEntity>(EntityModel<TEntity> model, params TEntity[] entities)
			where TEntity : class
		{
			return Create<TEntity>(model, model, entities);
		}

		public static InsertOperation Create<TEntity, TProjection>(EntityModel<TEntity> model, params TProjection[] projections)
			where TProjection : class
		{
			return Create<TProjection>(model.GetProjection<TProjection>(), model, projections);
		}

		private static InsertOperation Create<TProjection>(ProjectionModel projectionModel, EntityModel entityModel, params TProjection[] projections)
			where TProjection : class
		{
			var individualInsertExpressions = new List<IndividualInsert>();

			var primaryKeyField = entityModel.Fields.OfType<IValueField>().FirstOrDefault(q => q.Column.IsPrimaryKey);
			var primaryKeyIsOnProjection = false;
			var primaryKeyIsServerGenerated = primaryKeyField?.Column.IsServerGenerated ?? false;
			ServerGeneratedValue serverGeneratedPrimaryKey = null;
			if (primaryKeyField != null)
			{
				primaryKeyIsOnProjection = projectionModel.Fields.OfType<IValueField>().Any(q => q.Column.ColumnName == primaryKeyField.Column.ColumnName);
			}

			var transformer = new ColumnHelperTransformer(projectionModel);
			var columns = new List<ColumnHelper>();
			foreach (var projectionField in projectionModel.Fields)
			{
				projectionField.Transform(transformer);
				if (transformer.Current != null)
					columns.Add(transformer.Current);
			}
			if (!primaryKeyIsOnProjection && primaryKeyField != null)
			{
				primaryKeyField.Transform(transformer);
				if (transformer.Current != null)
					columns.Add(transformer.Current);
			}
			if (primaryKeyField != null)
				serverGeneratedPrimaryKey = columns.OfType<ServerGeneratedValue>().FirstOrDefault(q => q.ColumnName == primaryKeyField.Column.ColumnName);

			var bulkInsertRows = new List<QueryExpression[]>();
			foreach (var obj in projections)
			{
				var readWriter = new ObjectReadWriter(obj, projectionModel, typeof(TProjection));
				var row = new QueryExpression[columns.Count];
				var i = 0;
				foreach (var column in columns)
				{
					row[i++] = column.GetColumnExpression(readWriter);
				}

				var mapBackPrimaryKey = false;
				if (primaryKeyIsOnProjection && primaryKeyIsServerGenerated)
				{
					mapBackPrimaryKey = !serverGeneratedPrimaryKey.HasValue(readWriter);
				}

				if (mapBackPrimaryKey)
				{
					individualInsertExpressions.Add(new IndividualInsert(new CompositeQueryExpression(
						QueryExpression.Insert(entityModel.EntityTable.TableName, columns.Select(q => q.ColumnName).ToArray(), row),
						QueryExpression.Select(
							new[] {
								QueryExpression.Alias(QueryExpression.LastInsertIdFunction(), primaryKeyField.Column.ColumnName)
							}
						)), readWriter, serverGeneratedPrimaryKey));
				}
				else
				{
					bulkInsertRows.Add(row);
				}
			}

			return new InsertOperation(
				bulkInsertRows.Count > 0 ? QueryExpression.Insert(entityModel.EntityTable.TableName, columns.Select(q => q.ColumnName).ToArray(), bulkInsertRows.ToArray()) : null,
				individualInsertExpressions);
		}

		private class IndividualInsert
		{
			public QueryExpression Query { get; }

			private readonly IModelReadWriter _modelReadWriter;
			private readonly ServerGeneratedValue[] _serverGeneratedValues;

			public IndividualInsert(QueryExpression query, IModelReadWriter modelReadWriter,
				params ServerGeneratedValue[] serverGeneratedValues)
			{
				Query = query;
				_modelReadWriter = modelReadWriter;
				_serverGeneratedValues = serverGeneratedValues;
			}

			public void MapOntoObject(QueryResult queryResult)
			{
				foreach (var serverGeneratedValue in _serverGeneratedValues)
				{
					serverGeneratedValue.ReadResultValue(queryResult, _modelReadWriter);
				}
			}
		}

		private abstract class ColumnHelper
		{
			public string ColumnName { get; }

			public ColumnHelper(string columnName)
			{
				ColumnName = columnName;
			}

			public abstract QueryExpression GetColumnExpression(IModelReadWriter modelReadWriter);
		}

		private class ColumnValueReader<T> : ColumnHelper
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

		private class ClientGeneratedValue<T> : ColumnHelper
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

		private abstract class ServerGeneratedValue : ColumnHelper
		{
			public ServerGeneratedValue(string columnName) : base(columnName)
			{
			}

			public abstract bool HasValue(IModelReadWriter modelReadWriter);

			public abstract void ReadResultValue(QueryResult queryResult, IModelReadWriter modelReadWriter);
		}

		private class ServerGeneratedValue<T> : ServerGeneratedValue
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

		private class ColumnHelperTransformer : IModelTransformer
		{
			private readonly ProjectionModel _projectionModel;

			public ColumnHelper Current { get; private set; }

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
				Current = null;

				switch (field)
				{
					case IValueField valueField:
						{
							var column = valueField.Column;
							var fieldPath = new[] { valueField.FieldName };
							if (!FieldIsOnModel(fieldPath))
								fieldPath = null;
							if (column.IsClientGenerated)
								Current = new ClientGeneratedValue<T>(valueField.Column.ColumnName, fieldPath);
							else if (column.IsServerGenerated)
								Current = new ServerGeneratedValue<T>(valueField.Column.ColumnName, fieldPath);
							else
								Current = new ColumnValueReader<T>(valueField.Column.ColumnName, fieldPath);
						}
						break;
					case ISingleRelatedObjectField singleRelatedObjectField:
						{
							var visitor = new SingleObjectFieldVisitor(this, singleRelatedObjectField);
							singleRelatedObjectField.RelatedPrimaryKey.Transform(visitor);
						}
						break;
				}
			}

			public void VisitModel<TField>(IModel<TField> model) where TField : IField
			{
				throw new System.NotImplementedException();
			}

			private class SingleObjectFieldVisitor : IModelTransformer
			{
				private readonly ColumnHelperTransformer _parent;
				private readonly ISingleRelatedObjectField _field;

				public SingleObjectFieldVisitor(ColumnHelperTransformer parent, ISingleRelatedObjectField singleRelatedObjectField)
				{
					_parent = parent;
					_field = singleRelatedObjectField;
				}

				public void VisitField<T>(IField<T> field)
				{
					if (!(field is IValueField valueField))
						return;

					var column = valueField.Column;
					var fieldPath = new[] { _field.FieldName, valueField.FieldName };
					if (!_parent.FieldIsOnModel(fieldPath))
						fieldPath = null;
					_parent.Current = new ColumnValueReader<T>(_field.LocalColumn.ColumnName, fieldPath);
				}

				public void VisitModel<TField>(IModel<TField> model) where TField : IField
				{
					throw new NotImplementedException();
				}
			}
		}
	}
}
