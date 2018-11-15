using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class EntityInsertBuilder<T> : QueryBuilderBase<T>
		where T : class
	{
		private readonly List<string> _columnNames = new List<string>();
		private readonly List<List<QueryExpression>> _rowValues = new List<List<QueryExpression>>();
		private List<QueryExpression> _currentValueExpressions;

		public EntityInsertBuilder(Schema.Schema schema, ObjectReadWriter entityReadWriter = null)
			: base(schema, entityReadWriter)
		{
			_currentValueExpressions = new List<QueryExpression>();
			_rowValues.Add(_currentValueExpressions);
		}

		public EntityInsertBuilder(EntitySchema<T> schema, ObjectReadWriter entityReadWriter = null)
			: base(schema, entityReadWriter)
		{
			_currentValueExpressions = new List<QueryExpression>();
			_rowValues.Add(_currentValueExpressions);
		}

		private void AddColumnName(Column column)
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

				_currentValueExpressions = new List<QueryExpression>();
				_rowValues.Add(_currentValueExpressions);
			}
		}

		public void Set(ISchemaField<T> schemaField, T entity)
		{
			AddColumnName(schemaField.Column);
			var valueExpression = schemaField.ExpressionFactory.Value(entity, EntityReadWriter);
			_currentValueExpressions.Add(valueExpression);
		}

		public void Set<TValue>(ISchemaField<T> schemaField, TValue value)
		{
			AddColumnName(schemaField.Column);
			var valueExpression = QueryExpression.Value(value);
			_currentValueExpressions.Add(valueExpression);
		}

		public override QueryExpression BuildQuery()
		{
			var valuesArray = _rowValues.Select(row => row.Select(value => value).ToArray()).ToArray();
			return QueryExpression.Insert(
				Source.TableName,
				_columnNames.ToArray(),
				valuesArray
				);
		}
	}

	public class InsertBuilder<TLeft, TRight> : QueryBuilderBase<TLeft, TRight>
		where TLeft : class
		where TRight : class
	{
		private readonly List<List<FieldAssignment>> _fieldAssignments = new List<List<FieldAssignment>>()
		{
			new List<FieldAssignment>()
		};

		public InsertBuilder(Schema.Schema schema, string relationshipName) : base(schema, relationshipName) { }

		public InsertBuilder(Relationship<TLeft, TRight> relationship) : base(relationship) { }

		public void NewRow()
		{
			if (_fieldAssignments.Last().Count > 0)
				_fieldAssignments.Add(new List<FieldAssignment>());
		}

		public void Set(FieldAssignment fieldValuePair)
		{
			_fieldAssignments.Last().Add(fieldValuePair);
		}

		public override QueryExpression BuildQuery()
		{
			return QueryExpression.Insert(
				Source.TableName,
				_fieldAssignments.First().SelectMany(q => q.GetColumnExpressionPairs()).Select(q => q.ColumnExpression.ColumnName).ToArray(),
				_fieldAssignments.Select(row =>
					row.SelectMany(fieldValuePair => fieldValuePair.GetColumnExpressionPairs())
						.Select(columnPair => columnPair.ValueExpression).ToArray()
					).ToArray()
				);
		}
	}
}
