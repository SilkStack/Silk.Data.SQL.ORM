using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class EntityInsertBuilder<T> : QueryBuilderBase<T>
		where T : class
	{
		private readonly List<List<FieldAssignment>> _fieldAssignments = new List<List<FieldAssignment>>()
		{
			new List<FieldAssignment>()
		};

		public EntityInsertBuilder(Schema.Schema schema) : base(schema) { }

		public EntityInsertBuilder(EntitySchema<T> schema) : base(schema) { }

		public void NewRow()
		{
			if (_fieldAssignments.Last().Count > 0)
				_fieldAssignments.Add(new List<FieldAssignment>());
		}

		//public void Set(FieldAssignment fieldValuePair)
		//{
		//	_fieldAssignments.Last().Add(fieldValuePair);
		//}

		public void Set(ISchemaField<T> schemaField, T entity)
		{

		}

		public void Set<TValue>(ISchemaField<T> entityField, TValue value)
		{
			//Set(new FieldValueAssignment<TValue>(entityField, new StaticValueReader<TValue>(value)));
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
