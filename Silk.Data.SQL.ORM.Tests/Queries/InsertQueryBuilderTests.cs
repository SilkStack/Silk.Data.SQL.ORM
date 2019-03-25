using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using System.Linq;

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

		[TestMethod]
		public void Create_View_Maps_Converted_Fields()
		{
			var schema = new SchemaBuilder()
				.Define<EntityModel>()
				.AddTypeConverters(new[] { new SubConverter() })
				.Build();
			var view = new ViewModel
			{
				Sub = new ViewModelSub { CustomData = "Hello World" }
			};
			var query = InsertBuilder<EntityModel>.Create(schema, view)
				.BuildQuery() as InsertExpression;
			Assert.IsNotNull(query);
			Assert.IsTrue(query.Columns.Any(q => q.ColumnName == "Sub_Data"));
			var valueExpr = query.RowsExpressions[0][0] as ValueExpression;
			Assert.AreEqual(view.Sub.CustomData, valueExpr.Value);
		}

		private class SimpleEntity
		{
			public string Property { get; set; }
		}

		private class ServerPrimaryKey
		{
			public int Id { get; set; }
		}

		private class EntityModel
		{
			public EntityModelSub Sub { get; set; }
		}

		private class EntityModelSub
		{
			public string Data { get; set; }
		}

		private class ViewModel
		{
			public ViewModelSub Sub { get; set; }
		}

		private class ViewModelSub
		{
			public string CustomData { get; set; }
		}

		private class SubConverter : TypeConverter<ViewModelSub, EntityModelSub>
		{
			public override bool TryConvert(ViewModelSub from, out EntityModelSub to)
			{
				to = new EntityModelSub { Data = from.CustomData };
				return true;
			}
		}
	}
}
