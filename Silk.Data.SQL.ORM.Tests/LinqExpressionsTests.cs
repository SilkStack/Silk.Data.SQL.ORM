using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Queries;
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
		public void ConvertEmbeddedColumn()
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
		public void ConvertManyToOneColumn()
		{
			var expressionConverter = CreateParentConverter<int, int>(true);
			var condition = expressionConverter.Convert(q => q.Child1.A);

			var checkExpression = condition.QueryExpression as ColumnExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("A", checkExpression.ColumnName);
			Assert.IsNotNull(condition.RequiredJoins);
			Assert.AreEqual(1, condition.RequiredJoins.Length);
			Assert.AreEqual("__joinAlias_Child1", condition.RequiredJoins[0].TableAlias);

			condition = expressionConverter.Convert(q => q.Child2.A);

			checkExpression = condition.QueryExpression as ColumnExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("A", checkExpression.ColumnName);
			Assert.IsNotNull(condition.RequiredJoins);
			Assert.AreEqual(1, condition.RequiredJoins.Length);
			Assert.AreEqual("__joinAlias_Child2", condition.RequiredJoins[0].TableAlias);
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
		public void ConvertDatabaseFunctionWithRelatedField()
		{
			var expressionConverter = CreateParentConverter<TestEnum, TestEnum>(true);
			var condition = expressionConverter.Convert(q => q.Child1.A.HasFlag(TestEnum.OptionD));

			var checkExpression = condition.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.AreEqual, checkExpression.Operator);
			Assert.IsInstanceOfType(checkExpression.Left, typeof(BitwiseOperationQueryExpression));
			Assert.IsNotNull(condition.RequiredJoins);
			Assert.AreEqual(1, condition.RequiredJoins.Length);
			Assert.AreEqual("__joinAlias_Child1", condition.RequiredJoins[0].TableAlias);
		}

		[TestMethod]
		public void DontConvertQueryExpressions()
		{
			var expressionConverter = CreateConverter<int, int>();
			var queryExpression = QueryExpression.Select(QueryExpression.All(), QueryExpression.Table("TestTable"));
			var condition = expressionConverter.Convert(q => queryExpression);

			var checkExpression = condition.QueryExpression as SelectExpression;
			Assert.IsNotNull(checkExpression);
			Assert.ReferenceEquals(queryExpression, checkExpression);
		}

		[TestMethod]
		public void ConvertQueryBuilderToExpression()
		{
			var expressionConverter = CreateConverter<int, int>();
			var selectBuilder = new SelectBuilder<TestTuple<int, int>>(expressionConverter.Schema);
			var condition = expressionConverter.Convert(q => selectBuilder);

			var checkExpression = condition.QueryExpression as SelectExpression;
			Assert.IsNotNull(checkExpression);
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

		[TestMethod]
		public void EnumsAreIntegers()
		{
			var expressionConverter = CreateConverter<TestEnum, TestEnum>();
			var condition = expressionConverter.Convert(q => TestEnum.OptionC);

			var checkExpression = condition.QueryExpression as ValueExpression;
			Assert.IsNotNull(checkExpression);
			Assert.IsInstanceOfType(checkExpression.Value, typeof(int));
		}

		[TestMethod]
		public void ConvertHasFlagMethod()
		{
			var expressionConverter = CreateConverter<TestEnum, TestEnum>();
			var condition = expressionConverter.Convert(q => q.A.HasFlag(TestEnum.OptionB));

			var checkExpression = condition.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.AreEqual, checkExpression.Operator);
			Assert.IsInstanceOfType(checkExpression.Left, typeof(BitwiseOperationQueryExpression));
		}

		[TestMethod]
		public void ConvertLikeMethod()
		{
			var expressionConverter = CreateConverter<string, string>();
			var condition = expressionConverter.Convert(q => DatabaseFunctions.Like(q.A, "%search%"));

			var checkExpression = condition.QueryExpression as ComparisonExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual(ComparisonOperator.Like, checkExpression.Operator);
		}

		[TestMethod]
		public void ConvertManyToOneRelationshipKey()
		{
			var expressionConverter = CreateParentConverter<int, int>(true);
			var condition = expressionConverter.Convert(q => q.Child1.Id);

			var checkExpression = condition.QueryExpression as ColumnExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("FK_Child1_Id", checkExpression.ColumnName);
			Assert.AreEqual(0, condition.RequiredJoins.Length);

			condition = expressionConverter.Convert(q => q.Child2.Id);

			checkExpression = condition.QueryExpression as ColumnExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("FK_Child2_Id", checkExpression.ColumnName);
			Assert.AreEqual(0, condition.RequiredJoins.Length);
		}

		[TestMethod]
		public void ConvertRelationshipKeys()
		{
			var expressionConverter = CreateRelationshipConverter<int, int>();
			var conditionOne = expressionConverter.Convert((one, two) => one.Id);
			var conditionTwo = expressionConverter.Convert((one, two) => two.Id);

			var checkExpressionOne = conditionOne.QueryExpression as ColumnExpression;
			var checkExpressionTwo = conditionTwo.QueryExpression as ColumnExpression;
			Assert.IsNotNull(checkExpressionOne);
			Assert.IsNotNull(checkExpressionTwo);
			Assert.AreEqual(0, conditionOne.RequiredJoins.Length);
			Assert.AreEqual(0, conditionTwo.RequiredJoins.Length);
			Assert.AreEqual("FK_LeftTable_Id", checkExpressionOne.ColumnName);
			Assert.AreEqual("FK_RightTable_Id", checkExpressionTwo.ColumnName);
		}

		[TestMethod]
		public void ConvertRelatedColumn()
		{
			var expressionConverter = CreateRelationshipConverter<int, int>();
			var condition = expressionConverter.Convert((one, two) => one.A);

			var checkExpression = condition.QueryExpression as ColumnExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("A", checkExpression.ColumnName);
			Assert.AreEqual(1, condition.RequiredJoins.Length);
			Assert.AreEqual("Relationship_LeftTable", condition.RequiredJoins[0].TableAlias);

			condition = expressionConverter.Convert((one, two) => two.A);

			checkExpression = condition.QueryExpression as ColumnExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("A", checkExpression.ColumnName);
			Assert.AreEqual(1, condition.RequiredJoins.Length);
			Assert.AreEqual("Relationship_RightTable", condition.RequiredJoins[0].TableAlias);
		}

		[TestMethod]
		public void ConvertSingleJoinThroughRelationship()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<TestTupleTwo<int, int>>().TableName = "LeftTable";
			schemaBuilder.DefineEntity<TupleParent<int, int>>().TableName = "RightTable";
			schemaBuilder.DefineEntity<TestTuple<int, int>>().TableName = "ChildTable";
			schemaBuilder.DefineRelationship<TestTupleTwo<int, int>, TupleParent<int, int>>("Relationship");
			var schema = schemaBuilder.Build();
			var expressionConverter = new ExpressionConverter<TestTupleTwo<int, int>, TupleParent<int, int>>(schema, "Relationship");

			var condition = expressionConverter.Convert((tupleTwo, tupleParent) => tupleParent.Child1.A);

			var checkExpression = condition.QueryExpression as ColumnExpression;
			Assert.IsNotNull(checkExpression);
			Assert.AreEqual("A", checkExpression.ColumnName);
			Assert.AreEqual(2, condition.RequiredJoins.Length);
			Assert.AreEqual("Relationship_RightTable", condition.RequiredJoins[0].TableAlias);
			Assert.AreEqual("__joinAlias_Child1", condition.RequiredJoins[1].TableAlias);
		}

		private ExpressionConverter<TestTuple<T1,T2>> CreateConverter<T1, T2>()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<TestTuple<T1, T2>>().TableName = "TestTable";
			var schema = schemaBuilder.Build();
			return new ExpressionConverter<TestTuple<T1, T2>>(schema);
		}

		private ExpressionConverter<TupleParent<T1, T2>> CreateParentConverter<T1, T2>(bool defineChildEntity)
		{
			var schemaBuilder = new SchemaBuilder();
			if (defineChildEntity)
				schemaBuilder.DefineEntity<TestTuple<T1, T2>>().TableName = "TestTable";
			schemaBuilder.DefineEntity<TupleParent<T1, T2>>().TableName = "ParentTable";
			var schema = schemaBuilder.Build();
			return new ExpressionConverter<TupleParent<T1, T2>>(schema);
		}

		private ExpressionConverter<TestTuple<T1, T2>, TestTupleTwo<T1, T2>> CreateRelationshipConverter<T1, T2>()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<TestTuple<T1, T2>>().TableName = "LeftTable";
			schemaBuilder.DefineEntity<TestTupleTwo<T1, T2>>().TableName = "RightTable";
			schemaBuilder.DefineRelationship<TestTuple<T1, T2>, TestTupleTwo<T1, T2>>("Relationship");
			var schema = schemaBuilder.Build();
			return new ExpressionConverter<TestTuple<T1, T2>, TestTupleTwo<T1, T2>>(schema, "Relationship");
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
