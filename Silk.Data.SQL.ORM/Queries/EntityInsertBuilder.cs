using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Queries
{
	/// <summary>
	/// An INSERT statement builder for entities of type T.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EntityInsertBuilder<T> : QueryBuilderBase<T>
		where T : class
	{
		private readonly List<string> _columnNames = new List<string>();
		private readonly List<Dictionary<string,QueryExpression>> _rowValues = new List<Dictionary<string,QueryExpression>>();
		private Dictionary<string, QueryExpression> _currentValueExpressions;

		public EntityInsertBuilder(Schema.Schema schema, ObjectReadWriter entityReadWriter = null)
			: base(schema, entityReadWriter)
		{
			_currentValueExpressions = new Dictionary<string, QueryExpression>();
			_rowValues.Add(_currentValueExpressions);
		}

		public EntityInsertBuilder(EntitySchema<T> schema, ObjectReadWriter entityReadWriter = null)
			: base(schema, entityReadWriter)
		{
			_currentValueExpressions = new Dictionary<string, QueryExpression>();
			_rowValues.Add(_currentValueExpressions);
		}

		private void AddColumnName(Column column)
		{
			if (_rowValues.Count != 1)
				return;
			_columnNames.Add(column.ColumnName);
		}

		private void AddColumnName(ColumnExpression column)
		{
			if (_rowValues.Count != 1)
				return;
			_columnNames.Add(column.ColumnName);
		}

		public void NewRow()
		{
			if (_currentValueExpressions.Count > 0)
			{
				if (_currentValueExpressions.Count != _columnNames.Count)
					throw new InvalidOperationException("Column cound and value count mismatch.");

				_currentValueExpressions = new Dictionary<string, QueryExpression>();
				_rowValues.Add(_currentValueExpressions);
			}
		}

		public void Set(ISchemaField<T> schemaField, T entity)
		{
			var fieldOperations = Schema.GetFieldOperations(schemaField);
			AddColumnName(schemaField.Column);
			var valueExpression = fieldOperations.Expressions.Value(entity, EntityReadWriter);
			_currentValueExpressions[schemaField.Column.ColumnName] = valueExpression;
		}

		public void Set<TValue>(ISchemaField<T> schemaField, TValue value)
		{
			AddColumnName(schemaField.Column);
			var valueExpression = QueryExpression.Value(value);
			_currentValueExpressions[schemaField.Column.ColumnName] = valueExpression;
		}

		public void Set<TValue>(ISchemaField<T> schemaField, Expression<Func<T, TValue>> valueExpression)
		{
			AddColumnName(schemaField.Column);
			var valueExpressionResult = ExpressionConverter.Convert(valueExpression);
			_currentValueExpressions[schemaField.Column.ColumnName] = valueExpressionResult.QueryExpression;
		}

		public void Set<TValue>(ISchemaField<T> schemaField, IQueryBuilder subQuery)
		{
			AddColumnName(schemaField.Column);
			_currentValueExpressions[schemaField.Column.ColumnName] = subQuery.BuildQuery();
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, TProperty value)
		{
			var columnExpression = ExpressionConverter.Convert(fieldSelector)
				.QueryExpression as ColumnExpression;
			if (columnExpression == null)
				throw new InvalidOperationException("Invalid property selector expression.");
			AddColumnName(columnExpression);
			var valueExpression = QueryExpression.Value(value);
			_currentValueExpressions[columnExpression.ColumnName] = valueExpression;
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, Expression<Func<T, TProperty>> valueExpression)
		{
			var columnExpression = ExpressionConverter.Convert(fieldSelector)
				.QueryExpression as ColumnExpression;
			if (columnExpression == null)
				throw new InvalidOperationException("Invalid property selector expression.");
			AddColumnName(columnExpression);
			var valueExpressionResult = ExpressionConverter.Convert(valueExpression);
			_currentValueExpressions[columnExpression.ColumnName] = valueExpressionResult.QueryExpression;
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, Expression valueExpression)
		{
			var columnExpression = ExpressionConverter.Convert(fieldSelector)
				.QueryExpression as ColumnExpression;
			if (columnExpression == null)
				throw new InvalidOperationException("Invalid property selector expression.");
			AddColumnName(columnExpression);
			var valueExpressionResult = ExpressionConverter.Convert(valueExpression);
			_currentValueExpressions[columnExpression.ColumnName] = valueExpressionResult.QueryExpression;
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, IQueryBuilder subQuery)
		{
			var columnExpression = ExpressionConverter.Convert(fieldSelector)
				.QueryExpression as ColumnExpression;
			if (columnExpression == null)
				throw new InvalidOperationException("Invalid property selector expression.");
			AddColumnName(columnExpression);
			_currentValueExpressions[columnExpression.ColumnName] = subQuery.BuildQuery();
		}

		public void Set<TProperty>(Expression<Func<T, TProperty>> fieldSelector, QueryExpression queryExpression)
		{
			var columnExpression = ExpressionConverter.Convert(fieldSelector)
				.QueryExpression as ColumnExpression;
			if (columnExpression == null)
				throw new InvalidOperationException("Invalid property selector expression.");
			AddColumnName(columnExpression);
			_currentValueExpressions[columnExpression.ColumnName] = queryExpression;
		}

		public override QueryExpression BuildQuery()
		{
			return QueryExpression.Insert(
				Source.TableName,
				_columnNames.ToArray(),
				GetRows()
				);
		}

		private QueryExpression[][] GetRows()
		{
			var nameCount = _columnNames.Count;
			var rows = new QueryExpression[_rowValues.Count][];
			for (var i = 0; i < rows.Length; i++)
			{
				rows[i] = new QueryExpression[nameCount];
				for (var j = 0; j < nameCount; j++)
				{
					rows[i][j] = _rowValues[i][_columnNames[j]];
				}
			}
			return rows;
		}
	}
}
