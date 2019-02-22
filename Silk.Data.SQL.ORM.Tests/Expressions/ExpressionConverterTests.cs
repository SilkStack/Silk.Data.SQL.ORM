using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;

namespace Silk.Data.SQL.ORM.Tests.Expressions
{
	[TestClass]
	public class ExpressionConverterTests
	{
		[TestMethod]
		public void Convert_EntityTable_Returns_TableExpression()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q);

			var checkExpression = condition.QueryExpression as TableExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("TestTable", checkExpression.TableName);
		}

		[TestMethod]
		public void Convert_Constant_Returns_ValueExpression()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => true);

			var checkExpression = condition.QueryExpression as ValueExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(true, checkExpression.Value);
		}

		[TestMethod]
		public void Convert_Variable_Returns_ValueExpression()
		{
			var expressionConverter = CreateConverter<int, int>();
			var variable = true;
			var condition = expressionConverter.Convert(q => variable);

			var checkExpression = condition.QueryExpression as ValueExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(true, checkExpression.Value);
		}

		[TestMethod]
		public void Convert_Property_Returns_ValueExpression()
		{
			var expressionConverter = CreateConverter<int, int>();
			var obj = new Tuple<bool>(true);
			var condition = expressionConverter.Convert(q => obj.Item1);

			var checkExpression = condition.QueryExpression as ValueExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(true, checkExpression.Value);
		}

		[TestMethod]
		public void Convert_Local_Column_Returns_ColumnExpression()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A);

			var checkExpression = condition.QueryExpression as ColumnExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("A", checkExpression.ColumnName);
		}

		[TestMethod]
		public void Convert_Embedded_Column_Returns_ColumnExpression()
		{
			var expressionConverter = CreateParentConverter<int, int>(false);
			var condition = expressionConverter.Convert(q => q.Child1.A);

			var checkExpression = condition.QueryExpression as ColumnExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("Child1_A", checkExpression.ColumnName);

			condition = expressionConverter.Convert(q => q.Child2.A);

			checkExpression = condition.QueryExpression as ColumnExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("Child2_A", checkExpression.ColumnName);
		}

		[TestMethod]
		public void Convert_Many_To_One_Column_Returns_ColumnExpression()
		{
			var expressionConverter = CreateParentConverter<int, int>(true);
			var condition = expressionConverter.Convert(q => q.Child1.A);

			var checkExpression = condition.QueryExpression as ColumnExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("A", checkExpression.ColumnName);
			Assert.IsNotNull(condition.RequiredJoins);
			Assert.AreEqual(1, condition.RequiredJoins.Length);
			Assert.AreEqual("__join_1", condition.RequiredJoins[0].AliasIdentifierExpression.Identifier);

			condition = expressionConverter.Convert(q => q.Child2.A);

			checkExpression = condition.QueryExpression as ColumnExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("A", checkExpression.ColumnName);
			Assert.IsNotNull(condition.RequiredJoins);
			Assert.AreEqual(1, condition.RequiredJoins.Length);
			Assert.AreEqual("__join_2", condition.RequiredJoins[0].AliasIdentifierExpression.Identifier);
		}

		[TestMethod]
		public void Convert_Method_Parameter_Returns_ValueExpression()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = InternalConvert(true);

			var checkExpression = condition.QueryExpression as ValueExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(true, checkExpression.Value);

			ExpressionResult InternalConvert(bool param)
			{
				return expressionConverter.Convert(q => param);
			}
		}

		[TestMethod]
		public void Convert_HasFlag_On_RelatedEntity_Field_Returns_Bitwise_ComparisonExpression_With_Join()
		{
			var expressionConverter = CreateParentConverter<TestEnum, TestEnum>(true);
			var condition = expressionConverter.Convert(q => q.Child1.A.HasFlag(TestEnum.OptionD));

			var checkExpression = condition.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.AreEqual, checkExpression.Operator);
			Assert.IsInstanceOfType(checkExpression.Left, typeof(BitwiseOperationQueryExpression));
			Assert.IsNotNull(condition.RequiredJoins);
			Assert.AreEqual(1, condition.RequiredJoins.Length);
			Assert.AreEqual("__join_1", condition.RequiredJoins[0].AliasIdentifierExpression.Identifier);
		}

		private EntityExpressionConverter<TestTuple<T1, T2>> CreateConverter<T1, T2>()
		{
			return new EntityExpressionConverter<TestTuple<T1, T2>>(
				new SchemaBuilder()
					.Define<TestTuple<T1, T2>>(entity => entity.SetTableName("TestTable"))
					.Build()
				);
		}

		private EntityExpressionConverter<TupleParent<T1, T2>> CreateParentConverter<T1, T2>(bool defineChildEntity)
		{
			var schemaBuilder = new SchemaBuilder();
			if (defineChildEntity)
				schemaBuilder.Define<TestTuple<T1, T2>>(entity => entity.SetTableName("TestTable"));
			schemaBuilder.Define<TupleParent<T1, T2>>(entity => entity.SetTableName("ParentTable"));
			return new EntityExpressionConverter<TupleParent<T1, T2>>(schemaBuilder.Build());
		}

		private class TestTuple<T1, T2>
		{
			public Guid Id { get; private set; }
			public T1 A { get; set; }
			public T2 B { get; set; }
		}

		private class TestTupleTwo<T1, T2>
		{
			public Guid Id { get; private set; }
			public T1 A { get; set; }
			public T2 B { get; set; }
		}

		private class TupleParent<T1, T2>
		{
			public Guid Id { get; private set; }
			public TestTuple<T1, T2> Child1 { get; set; }
			public TestTuple<T1, T2> Child2 { get; set; }
		}

		[Flags]
		private enum TestEnum
		{
			None = 0,
			OptionA = 1,
			OptionB = 2,
			OptionC = 4,
			OptionD = 8,
			All = int.MaxValue
		}
	}
}
