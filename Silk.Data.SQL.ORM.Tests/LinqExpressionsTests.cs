using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class LinqExpressionsTests
	{
		[TestMethod]
		public void ConvertEntityTable()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q);

			var checkExpression = condition.QueryExpression as TableExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("TestTable", checkExpression.TableName);
		}

		[TestMethod]
		public void ConvertConstant()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => true);

			var checkExpression = condition.QueryExpression as ValueExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(true, checkExpression.Value);
		}

		[TestMethod]
		public void ConvertVariable()
		{
			var expressionConverter = CreateConverter<int, int>();
			var variable = true;
			var condition = expressionConverter.Convert(q => variable);

			var checkExpression = condition.QueryExpression as ValueExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(true, checkExpression.Value);
		}

		[TestMethod]
		public void ConvertProperty()
		{
			var expressionConverter = CreateConverter<int, int>();
			var obj = new Tuple<bool>(true);
			var condition = expressionConverter.Convert(q => obj.Item1);

			var checkExpression = condition.QueryExpression as ValueExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(true, checkExpression.Value);
		}

		[TestMethod]
		public void ConvertLocalColumn()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A);

			var checkExpression = condition.QueryExpression as ColumnExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("A", checkExpression.ColumnName);
		}

		[TestMethod]
		public void ConvertMethodParameter()
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
		public void OperatorEquality()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A == q.B);

			var checkExpression = condition.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.AreEqual, checkExpression.Operator);
		}

		[TestMethod]
		public void OperatorInequality()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A != q.B);

			var checkExpression = condition.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.AreNotEqual, checkExpression.Operator);
		}

		[TestMethod]
		public void OperatorGreateThan()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A > q.B);

			var checkExpression = condition.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.GreaterThan, checkExpression.Operator);
		}

		[TestMethod]
		public void OperatorGreaterThanOrEqualTo()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A >= q.B);

			var checkExpression = condition.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.GreaterThanOrEqualTo, checkExpression.Operator);
		}

		[TestMethod]
		public void OperatorLessThan()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A < q.B);

			var checkExpression = condition.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.LessThan, checkExpression.Operator);
		}

		[TestMethod]
		public void OperatorLessThanOrEqualTo()
		{
			var expressionConverter = CreateConverter<int,int>();
			var condition = expressionConverter.Convert(q => q.A <= q.B);

			var checkExpression = condition.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.LessThanOrEqualTo, checkExpression.Operator);
		}

		[TestMethod]
		public void ArithmaticAddition()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A + q.B);

			var checkExpression = condition.QueryExpression as ArithmaticQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ArithmaticOperator.Addition, checkExpression.Operator);
		}

		[TestMethod]
		public void ArithmaticSubtraction()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A - q.B);

			var checkExpression = condition.QueryExpression as ArithmaticQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ArithmaticOperator.Subtraction, checkExpression.Operator);
		}

		[TestMethod]
		public void ArithmaticMultiplication()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A * q.B);

			var checkExpression = condition.QueryExpression as ArithmaticQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ArithmaticOperator.Multiplication, checkExpression.Operator);
		}

		[TestMethod]
		public void ArithmaticDivision()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A / q.B);

			var checkExpression = condition.QueryExpression as ArithmaticQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ArithmaticOperator.Division, checkExpression.Operator);
		}

		[TestMethod]
		public void BitwiseAnd()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A & q.B);

			var checkExpression = condition.QueryExpression as BitwiseOperationQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(BitwiseOperator.And, checkExpression.Operator);
		}

		[TestMethod]
		public void BitwiseOr()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A | q.B);

			var checkExpression = condition.QueryExpression as BitwiseOperationQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(BitwiseOperator.Or, checkExpression.Operator);
		}

		[TestMethod]
		public void BitwiseXOr()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A ^ q.B);

			var checkExpression = condition.QueryExpression as BitwiseOperationQueryExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(BitwiseOperator.ExclusiveOr, checkExpression.Operator);
		}

		[TestMethod]
		public void ConditionAnd()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A == 1 && q.B == 1);
			
			var checkExpression = condition.QueryExpression as ConditionExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ConditionType.AndAlso, checkExpression.ConditionType);
		}

		[TestMethod]
		public void ConditionOr()
		{
			var expressionConverter = CreateConverter<int, int>();
			var condition = expressionConverter.Convert(q => q.A == 1 || q.B == 1);

			var checkExpression = condition.QueryExpression as ConditionExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ConditionType.OrElse, checkExpression.ConditionType);
		}

		private ExpressionConverter<TestTuple<T1,T2>> CreateConverter<T1, T2>()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<TestTuple<T1, T2>>().TableName = "TestTable";
			var schema = schemaBuilder.Build();
			return new ExpressionConverter<TestTuple<T1, T2>>(schema);
		}

		private class TestTuple<T1, T2>
		{
			public T1 A { get; set; }
			public T2 B { get; set; }
		}
	}
}
