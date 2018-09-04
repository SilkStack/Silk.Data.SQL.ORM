using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class InsertBuilder<T> : IQueryBuilder
		where T : class
	{
		private ExpressionConverter<T> _expressionConverter;
		private ExpressionConverter<T> ExpressionConverter
		{
			get
			{
				if (_expressionConverter == null)
					_expressionConverter = new ExpressionConverter<T>(Schema);
				return _expressionConverter;
			}
		}

		public Schema.Schema Schema { get; }
		public EntitySchema<T> EntitySchema { get; }

		private readonly List<List<FieldAssignment>> _fieldAssignments = new List<List<FieldAssignment>>()
		{
			new List<FieldAssignment>()
		};

		private readonly TableExpression _source;

		public InsertBuilder(Schema.Schema schema)
		{
			Schema = schema;
			EntitySchema = schema.GetEntitySchema<T>();
			if (EntitySchema == null)
				throw new Exception("Entity isn't configured in schema.");

			_source = QueryExpression.Table(EntitySchema.EntityTable.TableName);
		}

		public InsertBuilder(EntitySchema<T> schema)
		{
			EntitySchema = schema;
			Schema = schema.Schema;
			_source = QueryExpression.Table(EntitySchema.EntityTable.TableName);
		}

		public void NewRow()
		{
			if (_fieldAssignments.Last().Count > 0)
				_fieldAssignments.Add(new List<FieldAssignment>());
		}

		public void Set(FieldAssignment fieldValuePair)
		{
			_fieldAssignments.Last().Add(fieldValuePair);
		}

		public void Set<TValue>(EntityField<TValue, T> entityField, TValue value)
		{
			Set(new FieldValueAssignment<TValue>(entityField, new StaticValueReader<TValue>(value)));
		}

		public QueryExpression BuildQuery()
		{
			return QueryExpression.Insert(
				_source.TableName,
				_fieldAssignments.First().SelectMany(q => q.GetColumnExpressionPairs()).Select(q => q.Column.ColumnName).ToArray(),
				_fieldAssignments.Select(row =>
					row.SelectMany(fieldValuePair => fieldValuePair.GetColumnExpressionPairs())
						.Select(columnPair => columnPair.ValueExpression).ToArray()
					).ToArray()
				);
		}
	}
}
