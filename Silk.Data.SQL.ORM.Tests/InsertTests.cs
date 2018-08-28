using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Providers;
using Silk.Data.SQL.Queries;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class InsertTests
	{
		[TestMethod]
		public async Task InsertPrimitives()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<Primitives>();
			var schema = schemaBuilder.Build();
			var entitySchema = schema.GetEntitySchema<Primitives>();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<Primitives>(schema, provider);

				var insertBuilder = new EntityInsertBuilder<Primitives>(schema);
				var obj = new Primitives
				{
					Bool = true,
					Byte = 1,
					Short = 1,
					Int = 1,
					Long = 1,
					Float = 1.0f,
					Double = 1.0d,
					Decimal = 1.0m,
					String = "Hello World",
					DateTime = DateTime.Today,
					Guid = Guid.NewGuid()
				};
				insertBuilder.Add(obj);

				var queryExpression = insertBuilder.BuildQuery();
				await provider.ExecuteNonQueryAsync(queryExpression);

				using (var queryResult = await provider.ExecuteReaderAsync(
					QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(entitySchema.EntityTable.TableName))
					))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					CheckValue(
						obj.Bool, queryResult,
						entitySchema.EntityFields.First(q => q.ModelField.FieldName == nameof(obj.Bool)).Columns.First());
					CheckValue(
						obj.Byte, queryResult,
						entitySchema.EntityFields.First(q => q.ModelField.FieldName == nameof(obj.Byte)).Columns.First());
					CheckValue(
						obj.DateTime, queryResult,
						entitySchema.EntityFields.First(q => q.ModelField.FieldName == nameof(obj.DateTime)).Columns.First());
					CheckValue(
						obj.Decimal, queryResult,
						entitySchema.EntityFields.First(q => q.ModelField.FieldName == nameof(obj.Decimal)).Columns.First());
					CheckValue(
						obj.Double, queryResult,
						entitySchema.EntityFields.First(q => q.ModelField.FieldName == nameof(obj.Double)).Columns.First());
					CheckValue(
						obj.Float, queryResult,
						entitySchema.EntityFields.First(q => q.ModelField.FieldName == nameof(obj.Float)).Columns.First());
					CheckValue(
						obj.Guid, queryResult,
						entitySchema.EntityFields.First(q => q.ModelField.FieldName == nameof(obj.Guid)).Columns.First());
					CheckValue(
						obj.Int, queryResult,
						entitySchema.EntityFields.First(q => q.ModelField.FieldName == nameof(obj.Int)).Columns.First());
					CheckValue(
						obj.Long, queryResult,
						entitySchema.EntityFields.First(q => q.ModelField.FieldName == nameof(obj.Long)).Columns.First());
					CheckValue(
						obj.Short, queryResult,
						entitySchema.EntityFields.First(q => q.ModelField.FieldName == nameof(obj.Short)).Columns.First());
					CheckValue(
						obj.String, queryResult,
						entitySchema.EntityFields.First(q => q.ModelField.FieldName == nameof(obj.String)).Columns.First());
				}
			}
		}

		[TestMethod]
		public async Task InsertGuidPrimaryKey()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<HasGuidPK>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<HasGuidPK>(schema, provider);

				var obj = new HasGuidPK
				{
					Data = "Hello World"
				};

				var insertBuilder = new EntityInsertBuilder<HasGuidPK>(schema);
				var resultMapper = insertBuilder.Add(obj);
				Assert.IsNull(resultMapper);

				var queryExpression = insertBuilder.BuildQuery();
				using (var queryResult = await provider.ExecuteReaderAsync(queryExpression))
				{
					Assert.IsFalse(queryResult.HasRows);
					Assert.AreNotEqual(Guid.Empty, obj.Id);
				}
			}
		}

		[TestMethod]
		public async Task InsertIdPrimaryKey()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<HasIntPK>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				await CreateSchema<HasIntPK>(schema, provider);

				var obj1 = new HasIntPK
				{
					Data = "Hello"
				};
				var obj2 = new HasIntPK
				{
					Data = "World"
				};

				var insertBuilder = new EntityInsertBuilder<HasIntPK>(schema);
				var resultMapper = insertBuilder.Add(obj1, obj2);
				Assert.IsNotNull(resultMapper);
				Assert.AreEqual(2, resultMapper.ResultSetCount);

				var queryExpression = insertBuilder.BuildQuery();
				using (var queryResult = await provider.ExecuteReaderAsync(queryExpression))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());
					resultMapper.Inject(obj1, queryResult);
					Assert.IsTrue(await queryResult.NextResultAsync());
					Assert.IsTrue(await queryResult.ReadAsync());
					resultMapper.Inject(obj2, queryResult);
					Assert.AreNotEqual(0, obj1.Id);
					Assert.AreNotEqual(0, obj2.Id);
					Assert.AreNotEqual(obj2.Id, obj1.Id);
				}
			}
		}

		private void CheckValue<T>(T value, QueryResult queryResult, Column column)
		{
			Assert.AreEqual(value, queryResult.GetColumnValue(column));
		}

		private async Task CreateSchema<T>(Schema.Schema schema, IDataProvider dataProvider)
			where T : class
		{
			var createSchema = new EntityCreateSchemaBuilder<T>(schema);
			await dataProvider.ExecuteNonQueryAsync(createSchema.BuildQuery());
		}

		private class Primitives
		{
			public bool Bool { get; set; }
			public byte Byte { get; set; }
			public short Short { get; set; }
			public int Int { get; set; }
			public long Long { get; set; }
			public string String { get; set; }
			public DateTime DateTime { get; set; }
			public Guid Guid { get; set; }
			public float Float { get; set; }
			public double Double { get; set; }
			public decimal Decimal { get; set; }
		}

		private class HasGuidPK
		{
			public Guid Id { get; private set; }
			public string Data { get; set; }
		}

		private class HasIntPK
		{
			public int Id { get; private set; }
			public string Data { get; set; }
		}
	}
}
