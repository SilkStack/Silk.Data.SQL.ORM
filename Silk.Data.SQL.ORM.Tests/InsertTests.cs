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

				var query = schema.CreateInsert(obj);
				await provider.ExecuteAsync(schema.CreateTable<Primitives>(), query);

				using (var queryResult = await provider.ExecuteReaderAsync(
					QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(entitySchema.EntityTable.TableName))
					))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					CheckValue(
						obj.Bool, queryResult,
						entitySchema.EntityFields.First(q => q.FieldName == nameof(obj.Bool)).Columns.First());
					CheckValue(
						obj.Byte, queryResult,
						entitySchema.EntityFields.First(q => q.FieldName == nameof(obj.Byte)).Columns.First());
					CheckValue(
						obj.DateTime, queryResult,
						entitySchema.EntityFields.First(q => q.FieldName == nameof(obj.DateTime)).Columns.First());
					CheckValue(
						obj.Decimal, queryResult,
						entitySchema.EntityFields.First(q => q.FieldName == nameof(obj.Decimal)).Columns.First());
					CheckValue(
						obj.Double, queryResult,
						entitySchema.EntityFields.First(q => q.FieldName == nameof(obj.Double)).Columns.First());
					CheckValue(
						obj.Float, queryResult,
						entitySchema.EntityFields.First(q => q.FieldName == nameof(obj.Float)).Columns.First());
					CheckValue(
						obj.Guid, queryResult,
						entitySchema.EntityFields.First(q => q.FieldName == nameof(obj.Guid)).Columns.First());
					CheckValue(
						obj.Int, queryResult,
						entitySchema.EntityFields.First(q => q.FieldName == nameof(obj.Int)).Columns.First());
					CheckValue(
						obj.Long, queryResult,
						entitySchema.EntityFields.First(q => q.FieldName == nameof(obj.Long)).Columns.First());
					CheckValue(
						obj.Short, queryResult,
						entitySchema.EntityFields.First(q => q.FieldName == nameof(obj.Short)).Columns.First());
					CheckValue(
						obj.String, queryResult,
						entitySchema.EntityFields.First(q => q.FieldName == nameof(obj.String)).Columns.First());
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
				var obj = new HasGuidPK
				{
					Data = "Hello World"
				};

				await provider.ExecuteAsync(
					schema.CreateTable<HasGuidPK>(),
					schema.CreateInsert(obj)
					);
				Assert.AreNotEqual(Guid.Empty, obj.Id);
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
				var obj1 = new HasIntPK
				{
					Data = "Hello"
				};
				var obj2 = new HasIntPK
				{
					Data = "World"
				};

				await provider.ExecuteAsync(
					schema.CreateTable<HasIntPK>(),
					schema.CreateInsert(obj1, obj2)
					);

				Assert.AreNotEqual(0, obj1.Id);
				Assert.AreNotEqual(0, obj2.Id);
				Assert.AreNotEqual(obj2.Id, obj1.Id);
			}
		}

		[TestMethod]
		public async Task InsertEmbedded()
		{
			var tableName = default(string);
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<HasRelationship>(builder =>
			{
				tableName = builder.TableName;
				builder.For(q => q.Sub.Id).IsPrimaryKey = false;
			});
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				var obj = new HasRelationship
				{
					Data = "Hello",
					Sub = new HasGuidPK
					{
						Data = "World"
					}
				};

				await provider.ExecuteAsync(
					schema.CreateTable<HasRelationship>(),
					schema.CreateInsert(obj)
					);

				Assert.AreNotEqual(Guid.Empty, obj.Id);
				Assert.AreEqual(Guid.Empty, obj.Sub.Id);

				using (var queryResult = await provider.ExecuteReaderAsync(
					QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(tableName))
					))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(obj.Id, queryResult.GetGuid(queryResult.GetOrdinal("Id")));
					Assert.AreEqual(obj.Data, queryResult.GetString(queryResult.GetOrdinal("Data")));
					Assert.AreEqual(true, queryResult.GetBoolean(queryResult.GetOrdinal("Sub")));
					Assert.AreEqual(obj.Sub.Id, queryResult.GetGuid(queryResult.GetOrdinal("Sub_Id")));
					Assert.AreEqual(obj.Sub.Data, queryResult.GetString(queryResult.GetOrdinal("Sub_Data")));
				}
			}
		}

		[TestMethod]
		public async Task InsertWithRelationship()
		{
			var tableName = default(string);
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<HasGuidPK>();
			schemaBuilder.DefineEntity<HasRelationship>(builder =>
			{
				tableName = builder.TableName;
			});
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				var obj = new HasRelationship
				{
					Data = "Hello",
					Sub = new HasGuidPK
					{
						Data = "World"
					}
				};

				await provider.ExecuteAsync(
					schema.CreateTable<HasRelationship>(),
					schema.CreateTable<HasGuidPK>(),
					schema.CreateInsert(obj.Sub),
					schema.CreateInsert(obj)
					);

				using (var queryResult = await provider.ExecuteReaderAsync(
					QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(tableName))
					))
				{
					Assert.IsTrue(queryResult.HasRows);
					Assert.IsTrue(await queryResult.ReadAsync());

					Assert.AreEqual(obj.Id, queryResult.GetGuid(queryResult.GetOrdinal("Id")));
					Assert.AreEqual(obj.Data, queryResult.GetString(queryResult.GetOrdinal("Data")));
					Assert.AreEqual(obj.Sub.Id, queryResult.GetGuid(queryResult.GetOrdinal("FK_Sub_Id")));
				}
			}
		}

		private void CheckValue<T>(T value, QueryResult queryResult, Column column)
		{
			Assert.AreEqual(value, queryResult.GetColumnValue(column));
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

		private class HasRelationship
		{
			public Guid Id { get; private set; }
			public string Data { get; set; }
			public HasGuidPK Sub { get; set; }
		}
	}
}
