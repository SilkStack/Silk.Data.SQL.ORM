using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using System;

namespace Silk.Data.SQL.ORM.Tests.Queries
{
	[TestClass]
	public class CreateTableQueryBuilderTests
	{
		[TestMethod]
		public void Build_Query_Returns_Composite_Create_Query()
		{
			var entityModel = new EntityModel<object>(new[] {
				new ValueEntityField<int>("Field", true, true, new Column("Field", SqlDataType.Int(), false))
			});
			var createTableBuilder = new CreateTableQueryBuilder<object>(entityModel);
			var query = createTableBuilder.BuildQuery();

			Assert.IsInstanceOfType(query, typeof(CompositeQueryExpression));
		}

		[TestMethod]
		public void Build_Query_Creates_Valid_Create_Query()
		{
			var entityModel = new EntityModel<object>(new[] {
				new ValueEntityField<int>("Field", true, true, new Column("Field", SqlDataType.Int(), false))
			});
			var createTableBuilder = new CreateTableQueryBuilder<object>(entityModel);
			var query = createTableBuilder.BuildQuery() as CompositeQueryExpression;
			var createQuery = query.Queries[0] as CreateTableExpression;

			Assert.IsNotNull(createQuery);
			Assert.AreEqual("Object", createQuery.TableName);
			Assert.AreEqual(1, createQuery.ColumnDefinitions.Length);
			Assert.AreEqual("Field", createQuery.ColumnDefinitions[0].ColumnName);
			Assert.AreEqual(SqlDataType.Int(), createQuery.ColumnDefinitions[0].DataType);
			Assert.AreEqual(false, createQuery.ColumnDefinitions[0].IsNullable);
			Assert.AreEqual(false, createQuery.ColumnDefinitions[0].IsAutoIncrement);
			Assert.AreEqual(false, createQuery.ColumnDefinitions[0].IsPrimaryKey);
		}

		[TestMethod]
		public void Build_Query_Creates_Client_Generated_Primary_Key()
		{
			var entityModel = new EntityModel<object>(new[] {
				new ValueEntityField<Guid>("Id", true, true, new Column("Id", SqlDataType.Guid(), false))
			});
			var createTableBuilder = new CreateTableQueryBuilder<object>(entityModel);
			var query = createTableBuilder.BuildQuery() as CompositeQueryExpression;
			var createQuery = query.Queries[0] as CreateTableExpression;

			Assert.AreEqual("Id", createQuery.ColumnDefinitions[0].ColumnName);
			Assert.AreEqual(SqlDataType.Guid(), createQuery.ColumnDefinitions[0].DataType);
			Assert.AreEqual(false, createQuery.ColumnDefinitions[0].IsNullable);
			Assert.AreEqual(false, createQuery.ColumnDefinitions[0].IsAutoIncrement);
			Assert.AreEqual(true, createQuery.ColumnDefinitions[0].IsPrimaryKey);
		}

		[TestMethod]
		public void Build_Query_Creates_Server_Generated_Primary_Key()
		{
			var entityModel = new EntityModel<object>(new[] {
				new ValueEntityField<int>("Id", true, true, new Column("Id", SqlDataType.Int(), false))
			});
			var createTableBuilder = new CreateTableQueryBuilder<object>(entityModel);
			var query = createTableBuilder.BuildQuery() as CompositeQueryExpression;
			var createQuery = query.Queries[0] as CreateTableExpression;

			Assert.AreEqual("Id", createQuery.ColumnDefinitions[0].ColumnName);
			Assert.AreEqual(SqlDataType.Int(), createQuery.ColumnDefinitions[0].DataType);
			Assert.AreEqual(false, createQuery.ColumnDefinitions[0].IsNullable);
			Assert.AreEqual(true, createQuery.ColumnDefinitions[0].IsAutoIncrement);
			Assert.AreEqual(true, createQuery.ColumnDefinitions[0].IsPrimaryKey);
		}
	}
}
