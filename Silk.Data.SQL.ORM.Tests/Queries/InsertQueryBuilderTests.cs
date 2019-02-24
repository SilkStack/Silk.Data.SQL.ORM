using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Tests.Queries
{
	[TestClass]
	public class InsertQueryBuilderTests
	{
		[TestMethod]
		public void Create_From_Entity_Returns_Entity_Insert()
		{
			var schema = new SchemaBuilder()
				.Define<SimpleEntity>()
				.Build();
			var entity = new SimpleEntity { Property = "1234" };
			var query = InsertBuilder<SimpleEntity>.Create(schema, entity)
				.BuildQuery() as InsertExpression;

			Assert.IsNotNull(query);
			Assert.AreEqual("SimpleEntity", query.Table.TableName);
			Assert.AreEqual(1, query.Columns.Length);
			Assert.AreEqual("Property", query.Columns[0].ColumnName);
			Assert.AreEqual(1, query.RowsExpressions.Length);
			Assert.AreEqual(1, query.RowsExpressions[0].Length);
			Assert.AreEqual(entity.Property, ((ValueExpression)query.RowsExpressions[0][0]).Value);
		}

		[TestMethod]
		public void Create_From_View_Returns_Entity_Insert()
		{
			var schema = new SchemaBuilder()
				.Define<SimpleEntity>()
				.Build();
			var entity = new { Property = "1234" };
			var query = InsertBuilder<SimpleEntity>.Create(schema, entity)
				.BuildQuery() as InsertExpression;

			Assert.IsNotNull(query);
			Assert.AreEqual("SimpleEntity", query.Table.TableName);
			Assert.AreEqual(1, query.Columns.Length);
			Assert.AreEqual("Property", query.Columns[0].ColumnName);
			Assert.AreEqual(1, query.RowsExpressions.Length);
			Assert.AreEqual(1, query.RowsExpressions[0].Length);
			Assert.AreEqual(entity.Property, ((ValueExpression)query.RowsExpressions[0][0]).Value);
		}

		[TestMethod]
		public void Create_Ignores_Server_Generated_Primary_Keys()
		{
			var schema = new SchemaBuilder()
				.Define<ServerPrimaryKey>()
				.Build();
			var entity = new ServerPrimaryKey();
			var query = InsertBuilder<ServerPrimaryKey>.Create(schema, entity)
				.BuildQuery() as InsertExpression;

			Assert.AreEqual(0, query.Columns.Length);
			Assert.AreEqual(1, query.RowsExpressions.Length);
			Assert.AreEqual(0, query.RowsExpressions[0].Length);
		}

		private class SimpleEntity
		{
			public string Property { get; set; }
		}

		private class ServerPrimaryKey
		{
			public int Id { get; set; }
		}
	}
}
