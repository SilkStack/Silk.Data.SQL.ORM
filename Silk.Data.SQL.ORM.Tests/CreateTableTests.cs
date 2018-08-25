using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.ORM.Schema;
using Silk.Data.SQL.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class CreateTableTests
	{
		[TestMethod]
		public async Task CreatePrimitiveTable()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<Primitives>();
			var schema = schemaBuilder.Build();

			using (var provider = TestHelper.CreateProvider())
			{
				var queryBuilder = new EntityCreateSchemaBuilder<Primitives>(schema);
				var queryExpression = queryBuilder.BuildQuery();
				await provider.ExecuteNonQueryAsync(queryExpression);

				await InsertDefaultRow<Primitives>(provider, schema);
				await TestDefaultRow<Primitives>(provider, schema);
			}
		}

		private static async Task InsertDefaultRow<T>(IDataProvider dataProvider, Schema.Schema schema)
		{
			var entitySchema = schema.GetEntitySchema<T>();
			var expression = QueryExpression.Insert(
				entitySchema.EntityTable.TableName,
				entitySchema.EntityTable.Columns.Select(q => q.ColumnName).ToArray(),
				entitySchema.EntityTable.Columns.Select(q => GetDefaultValue(q)).ToArray()
				);
			await dataProvider.ExecuteNonQueryAsync(expression);
		}

		private static async Task TestDefaultRow<T>(IDataProvider dataProvider, Schema.Schema schema)
		{
			var entitySchema = schema.GetEntitySchema<T>();
			using (var queryResult = await dataProvider.ExecuteReaderAsync(
				QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(entitySchema.EntityTable.TableName))
				))
			{
				Assert.IsTrue(queryResult.HasRows);
				Assert.IsTrue(await queryResult.ReadAsync());
				foreach (var column in entitySchema.EntityTable.Columns)
				{
					Assert.AreEqual(GetDefaultValue(column), queryResult.GetColumnValue(column),
						$"Column value doesn't match expected value on column: {column.ColumnName}");
				}
			}
		}

		private static object GetDefaultValue(Column column)
		{
			switch (column.DataType.BaseType)
			{
				case SqlBaseType.Bit:
					return true;
				case SqlBaseType.BigInt:
					return 1L;
				case SqlBaseType.Int:
					return 1;
				case SqlBaseType.SmallInt:
					return (short)1;
				case SqlBaseType.TinyInt:
					return (byte)1;
				case SqlBaseType.Float:
					return 1.0d;
				case SqlBaseType.Date:
				case SqlBaseType.DateTime:
					return DateTime.Today;
				case SqlBaseType.Decimal:
					return 1.0m;
				case SqlBaseType.Guid:
					return Guid.Parse("{2E02EFD7-A0EE-40BD-A487-EDF2EA6D5CD5}");
				case SqlBaseType.Text:
					return "Hello World";
			}
			throw new Exception("Unsupported data type.");
		}

		private class Primitives
		{
			public bool Bool { get; set; }
			public sbyte SByte { get; set; }
			public byte Byte { get; set; }
			public ushort UShort { get; set; }
			public short Short { get; set; }
			public uint UInt { get; set; }
			public int Int { get; set; }
			public ulong ULong { get; set; }
			public long Long { get; set; }
			public string String { get; set; }
			public DateTime DateTime { get; set; }
			public Guid Guid { get; set; }
			public float Float { get; set; }
			public double Double { get; set; }
			public decimal Decimal { get; set; }
		}
	}
}
