using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Tests.Queries
{
	[TestClass]
	public class DeleteQueryBuilderTests
	{
		[TestMethod]
		public void Create_Without_Primary_Key_Throws_Exception()
		{
			var schema = new SchemaBuilder()
				.Define<NoPrimaryKey>()
				.Build();
			var entity = new NoPrimaryKey();
			var threw = false;

			try
			{
				var deleteQuery = DeleteBuilder<NoPrimaryKey>.Create(schema, entity);
			}
			catch
			{
				threw = true;
			}

			Assert.IsTrue(threw);
		}

		[TestMethod]
		public void Create_With_Primary_Key_Returns_Query()
		{
			var schema = new SchemaBuilder()
				.Define<HasPrimaryKey>()
				.Build();
			var entity = new HasPrimaryKey { Id = 1 };
			var deleteQuery = DeleteBuilder<HasPrimaryKey>.Create(schema, entity).BuildQuery() as DeleteExpression;

			Assert.IsNotNull(deleteQuery);
			Assert.AreEqual("HasPrimaryKey", deleteQuery.Table.TableName);

			var whereCondition = deleteQuery.WhereConditions as ComparisonExpression;
			Assert.AreEqual(ComparisonOperator.AreEqual, whereCondition.Operator);

			var field = whereCondition.Left as ColumnExpression;
			var value = whereCondition.Right as ValueExpression;
			Assert.AreEqual("Id", field.ColumnName);
			Assert.AreEqual(entity.Id, value.Value);
		}

		private class NoPrimaryKey
		{
			public string Property { get; set; }
		}

		private class HasPrimaryKey
		{
			public int Id { get; set; }
		}
	}
}
