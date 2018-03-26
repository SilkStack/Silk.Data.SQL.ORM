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
		private readonly List<QueryExpression> _individualInsertExpressions;

		public override bool CanBeBatched => !GeneratesValuesServerSide;

		public InsertOperation(QueryExpression bulkInsertExpression,
			List<QueryExpression> individualInsertExpressions)
		{
			GeneratesValuesServerSide = individualInsertExpressions.Count > 0;
			_bulkInsertExpression = bulkInsertExpression;
			_individualInsertExpressions = individualInsertExpressions;
		}

		public override QueryExpression GetQuery()
		{
			if (_individualInsertExpressions == null || _individualInsertExpressions.Count < 1)
				return _bulkInsertExpression;

			if (_bulkInsertExpression != null)
				return new CompositeQueryExpression(
					_individualInsertExpressions.Concat(new[] { _bulkInsertExpression }).ToArray()
					);
			return new CompositeQueryExpression(
				_individualInsertExpressions.ToArray()
				);
		}

		public override void ProcessResult(QueryResult queryResult)
		{
			if (_individualInsertExpressions == null || _individualInsertExpressions.Count < 1)
				return;

			foreach (var individualInsert in _individualInsertExpressions)
			{
				queryResult.NextResult();
			}
		}

		public override async Task ProcessResultAsync(QueryResult queryResult)
		{
			if (_individualInsertExpressions == null || _individualInsertExpressions.Count < 1)
				return;

			foreach (var individualInsert in _individualInsertExpressions)
			{
				await queryResult.NextResultAsync();
			}
		}

		public static InsertOperation Create<TEntity>(EntityModel<TEntity> model, params TEntity[] entities)
		{
			return Create<TEntity>(model, model, entities);
		}

		public static InsertOperation Create<TEntity, TProjection>(EntityModel<TEntity> model, params TProjection[] projections)
		{
			return Create<TProjection>(model.GetProjection<TProjection>(), model, projections);
		}

		private static InsertOperation Create<TProjection>(ProjectionModel projectionModel, EntityModel entityModel, params TProjection[] projections)
		{
			List<QueryExpression> individualInsertExpressions = new List<QueryExpression>();

			var primaryKeyField = entityModel.Fields.OfType<IValueField>().FirstOrDefault(q => q.Column.IsPrimaryKey);
			var primaryKeyIsOnProjection = false;
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
				bulkInsertRows.Add(row);
			}

			return new InsertOperation(
				bulkInsertRows.Count > 0 ? QueryExpression.Insert(entityModel.EntityTable.TableName, columns.Select(q => q.ColumnName).ToArray(), bulkInsertRows.ToArray()) : null,
				individualInsertExpressions);
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

		private class ColumnHelperTransformer : IModelTransformer
		{
			private readonly ProjectionModel _projectionModel;

			public ColumnHelper Current { get; private set; }

			public ColumnHelperTransformer(ProjectionModel projectionModel)
			{
				_projectionModel = projectionModel;
			}

			private bool FieldIsOnModel(IField field)
			{
				var foundField = _projectionModel.Fields.FirstOrDefault(q => q.FieldName == field.FieldName);
				return foundField != null;
			}

			public void VisitField<T>(IField<T> field)
			{
				Current = null;

				if (field is IValueField valueField)
				{
					var column = valueField.Column;
					var fieldPath = new[] { valueField.FieldName };
					if (!FieldIsOnModel(field))
						fieldPath = null;
					if (column.IsClientGenerated)
						Current = new ClientGeneratedValue<T>(valueField.Column.ColumnName, fieldPath);
					else if (column.IsServerGenerated)
						Current = null;
					else
						Current = new ColumnValueReader<T>(valueField.Column.ColumnName, fieldPath);
				}
			}

			public void VisitModel<TField>(IModel<TField> model) where TField : IField
			{
				throw new System.NotImplementedException();
			}
		}
	}
}
