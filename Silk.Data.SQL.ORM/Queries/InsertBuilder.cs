﻿using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class InsertBuilder<T> : QueryBuilderBase<T>
		where T : class
	{
		private readonly List<List<FieldAssignment>> _fieldAssignments = new List<List<FieldAssignment>>()
		{
			new List<FieldAssignment>()
		};

		public InsertBuilder(Schema.Schema schema) : base(schema) { }

		public InsertBuilder(EntitySchema<T> schema) : base(schema) { }

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

		public override QueryExpression BuildQuery()
		{
			return QueryExpression.Insert(
				Source.TableName,
				_fieldAssignments.First().SelectMany(q => q.GetColumnExpressionPairs()).Select(q => q.Column.ColumnName).ToArray(),
				_fieldAssignments.Select(row =>
					row.SelectMany(fieldValuePair => fieldValuePair.GetColumnExpressionPairs())
						.Select(columnPair => columnPair.ValueExpression).ToArray()
					).ToArray()
				);
		}
	}
}
