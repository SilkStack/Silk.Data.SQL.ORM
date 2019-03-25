using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using System.Linq;

namespace Silk.Data.SQL.ORM.Tests.Queries
{
	[TestClass]
	public class DefaultConditionBuilderTests
	{
		[TestMethod]
		public void AndAlso_Reads_Field_From_Entity()
		{
			var schema = new SchemaBuilder()
				.Define<Entity>()
				.Build();
			var builder = new DefaultEntityConditionBuilder<Entity>(
				schema
				);
			var field = builder.EntityModel.Fields.First();
			var testEntity = new Entity { Data = "TestValue" };
			builder.AndAlso(field, SQL.Expressions.ComparisonOperator.AreEqual, testEntity);
			var expr = builder.Build().QueryExpression as ComparisonExpression;

			Assert.IsNotNull(expr);
			var valueExpression = expr.Right as ValueExpression;
			Assert.IsNotNull(valueExpression);
			Assert.AreEqual(testEntity.Data, valueExpression.Value);
		}

		private class Entity
		{
			public string Data { get; set; }
		}
	}
}
