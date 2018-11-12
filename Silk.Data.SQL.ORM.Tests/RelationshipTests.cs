using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class RelationshipTests
	{
		[TestMethod]
		public async Task CreateRelationshipTable()
		{
			throw new NotImplementedException();
			//var schemaBuilder = new SchemaBuilder();
			//schemaBuilder.DefineEntity<SimplePoco1>();
			//schemaBuilder.DefineEntity<SimplePoco2>();
			//schemaBuilder.DefineRelationship<SimplePoco1, SimplePoco2>("Relationship");
			//var schema = schemaBuilder.Build();
			//var relationship = schema.GetRelationship<SimplePoco1, SimplePoco2>("Relationship");
			//var tableName = relationship.JunctionTable.TableName;
			//var columnNames = relationship.LeftRelationship.Columns.Select(q => q.ColumnName)
			//	.Concat(relationship.RightRelationship.Columns.Select(q => q.ColumnName))
			//	.ToArray();

			//using (var provider = TestHelper.CreateProvider())
			//{
			//	await provider.ExecuteAsync(
			//		schema.CreateTable<SimplePoco1>(),
			//		schema.CreateTable<SimplePoco2>(),
			//		schema.CreateTable<SimplePoco1, SimplePoco2>("Relationship")
			//		);

			//	await provider.ExecuteNonQueryAsync(QueryExpression.Insert(tableName, columnNames, new object[] { Guid.NewGuid(), Guid.NewGuid() }));
			//	using (var result = await provider.ExecuteReaderAsync(QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(tableName))))
			//	{
			//		Assert.IsTrue(result.HasRows);
			//	}
			//}
		}

		[TestMethod]
		public async Task CreateLeftToRightRelationships()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco1>();
			schemaBuilder.DefineEntity<SimplePoco2>();
			schemaBuilder.DefineRelationship<SimplePoco1, SimplePoco2>("Relationship");
			var schema = schemaBuilder.Build();
			var relationship = schema.GetRelationship<SimplePoco1, SimplePoco2>("Relationship");
			var tableName = relationship.JunctionTable.TableName;

			using (var provider = TestHelper.CreateProvider())
			{
				var inPoco1 = new SimplePoco1 { Data = 1 };
				var inPoco2s = new[]
				{
					new SimplePoco2 { Data = 2 },
					new SimplePoco2 { Data = 3 }
				};

				await provider.ExecuteAsync(
					schema.CreateTable<SimplePoco1>(),
					schema.CreateTable<SimplePoco2>(),
					relationship.CreateTable(),
					schema.CreateInsert(inPoco1),
					schema.CreateInsert(inPoco2s),
					relationship.CreateInsert(inPoco1, inPoco2s)
					);

				using (var result = await provider.ExecuteReaderAsync(QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(tableName))))
				{
					Assert.IsTrue(result.HasRows);
					var outPairs = new List<(Guid, Guid)>();
					foreach (var obj in inPoco2s)
					{
						Assert.IsTrue(await result.ReadAsync());
						outPairs.Add(
							(result.GetGuid(0), result.GetGuid(1))
							);
					}

					Assert.IsFalse(await result.ReadAsync());
					Assert.AreEqual(inPoco2s.Length, outPairs.Count);

					var foundPoco2s = new List<SimplePoco2>();
					foreach (var (poco1Id, poco2Id) in outPairs)
					{
						Assert.AreEqual(inPoco1.Id, poco1Id);

						var poco2 = inPoco2s.First(q => q.Id == poco2Id);
						foundPoco2s.Add(poco2);
					}

					foreach (var poco2 in inPoco2s)
					{
						Assert.IsTrue(foundPoco2s.Contains(poco2));
					}
				}
			}
		}

		[TestMethod]
		public async Task CreateRightToLeftRelationships()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco1>();
			schemaBuilder.DefineEntity<SimplePoco2>();
			schemaBuilder.DefineRelationship<SimplePoco1, SimplePoco2>("Relationship");
			var schema = schemaBuilder.Build();
			var relationship = schema.GetRelationship<SimplePoco1, SimplePoco2>("Relationship");
			var tableName = relationship.JunctionTable.TableName;

			using (var provider = TestHelper.CreateProvider())
			{
				var inPoco1s = new[]
				{
					new SimplePoco1 { Data = 1 },
					new SimplePoco1 { Data = 2 }
				};
				var inPoco2 = new SimplePoco2 { Data = 3 };

				await provider.ExecuteAsync(
					schema.CreateTable<SimplePoco1>(),
					schema.CreateTable<SimplePoco2>(),
					relationship.CreateTable(),
					schema.CreateInsert(inPoco1s),
					schema.CreateInsert(inPoco2),
					relationship.CreateInsert(inPoco2, inPoco1s)
					);

				using (var result = await provider.ExecuteReaderAsync(QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(tableName))))
				{
					Assert.IsTrue(result.HasRows);
					var outPairs = new List<(Guid, Guid)>();
					foreach (var obj in inPoco1s)
					{
						Assert.IsTrue(await result.ReadAsync());
						outPairs.Add(
							(result.GetGuid(0), result.GetGuid(1))
							);
					}

					Assert.IsFalse(await result.ReadAsync());
					Assert.AreEqual(inPoco1s.Length, outPairs.Count);

					var foundPoco1s = new List<SimplePoco1>();
					foreach (var (poco1Id, poco2Id) in outPairs)
					{
						Assert.AreEqual(inPoco2.Id, poco2Id);

						var poco1 = inPoco1s.First(q => q.Id == poco1Id);
						foundPoco1s.Add(poco1);
					}

					foreach (var poco1 in inPoco1s)
					{
						Assert.IsTrue(foundPoco1s.Contains(poco1));
					}
				}
			}
		}

		[TestMethod]
		public async Task DeleteAllLeftToRightRelationships()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco1>();
			schemaBuilder.DefineEntity<SimplePoco2>();
			schemaBuilder.DefineRelationship<SimplePoco1, SimplePoco2>("Relationship");
			var schema = schemaBuilder.Build();
			var relationship = schema.GetRelationship<SimplePoco1, SimplePoco2>("Relationship");
			var tableName = relationship.JunctionTable.TableName;

			using (var provider = TestHelper.CreateProvider())
			{
				var inPoco1s = new[]
				{
					new SimplePoco1 { Data = 1 },
					new SimplePoco1 { Data = 2 }
				};
				var inPoco2s = new[]
				{
					new SimplePoco2 { Data = 3 },
					new SimplePoco2 { Data = 4 }
				};

				await provider.ExecuteAsync(
					schema.CreateTable<SimplePoco1>(),
					schema.CreateTable<SimplePoco2>(),
					relationship.CreateTable(),
					schema.CreateInsert(inPoco1s),
					schema.CreateInsert(inPoco2s),
					relationship.CreateInsert(inPoco1s[0], inPoco2s),
					relationship.CreateInsert(inPoco1s[1], inPoco2s)
					);

				using (var result = await provider.ExecuteReaderAsync(QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(tableName))))
				{
					Assert.IsTrue(result.HasRows);

					var outPairs = new List<(Guid, Guid)>();
					while (await result.ReadAsync())
					{
						outPairs.Add(
							(result.GetGuid(0), result.GetGuid(1))
							);
					}

					Assert.AreEqual(4, outPairs.Count);
				}

				await provider.ExecuteAsync(relationship.CreateDelete(inPoco1s[0]));

				using (var result = await provider.ExecuteReaderAsync(QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(tableName))))
				{
					Assert.IsTrue(result.HasRows);

					var outPairs = new List<(Guid, Guid)>();
					while (await result.ReadAsync())
					{
						outPairs.Add(
							(result.GetGuid(0), result.GetGuid(1))
							);
					}

					Assert.AreEqual(2, outPairs.Count);
				}
			}
		}

		[TestMethod]
		public async Task DeleteSelectiveLeftToRightRelationships()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco1>();
			schemaBuilder.DefineEntity<SimplePoco2>();
			schemaBuilder.DefineRelationship<SimplePoco1, SimplePoco2>("Relationship");
			var schema = schemaBuilder.Build();
			var relationship = schema.GetRelationship<SimplePoco1, SimplePoco2>("Relationship");
			var tableName = relationship.JunctionTable.TableName;

			using (var provider = TestHelper.CreateProvider())
			{
				var inPoco1s = new[]
				{
					new SimplePoco1 { Data = 1 },
					new SimplePoco1 { Data = 2 },
					new SimplePoco1 { Data = 3 }
				};
				var inPoco2s = new[]
				{
					new SimplePoco2 { Data = 4 },
					new SimplePoco2 { Data = 5 },
					new SimplePoco2 { Data = 6 }
				};

				await provider.ExecuteAsync(
					schema.CreateTable<SimplePoco1>(),
					schema.CreateTable<SimplePoco2>(),
					relationship.CreateTable(),
					schema.CreateInsert(inPoco1s),
					schema.CreateInsert(inPoco2s),
					relationship.CreateInsert(inPoco1s[0], inPoco2s),
					relationship.CreateInsert(inPoco1s[1], inPoco2s),
					relationship.CreateInsert(inPoco1s[2], inPoco2s)
					);

				using (var result = await provider.ExecuteReaderAsync(QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(tableName))))
				{
					Assert.IsTrue(result.HasRows);

					var outPairs = new List<(Guid, Guid)>();
					while (await result.ReadAsync())
					{
						outPairs.Add(
							(result.GetGuid(0), result.GetGuid(1))
							);
					}

					Assert.AreEqual(9, outPairs.Count);
				}

				await provider.ExecuteAsync(relationship.CreateDelete(inPoco1s[0], inPoco2s[0], inPoco2s[1]));

				using (var result = await provider.ExecuteReaderAsync(QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(tableName))))
				{
					Assert.IsTrue(result.HasRows);

					var outPairs = new List<(Guid, Guid)>();
					while (await result.ReadAsync())
					{
						outPairs.Add(
							(result.GetGuid(0), result.GetGuid(1))
							);
					}

					Assert.AreEqual(7, outPairs.Count);
				}
			}
		}

		[TestMethod]
		public async Task DeleteAllRightToLeftRelationships()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco1>();
			schemaBuilder.DefineEntity<SimplePoco2>();
			schemaBuilder.DefineRelationship<SimplePoco1, SimplePoco2>("Relationship");
			var schema = schemaBuilder.Build();
			var relationship = schema.GetRelationship<SimplePoco1, SimplePoco2>("Relationship");
			var tableName = relationship.JunctionTable.TableName;

			using (var provider = TestHelper.CreateProvider())
			{
				var inPoco1s = new[]
				{
					new SimplePoco1 { Data = 1 },
					new SimplePoco1 { Data = 2 }
				};
				var inPoco2s = new[]
				{
					new SimplePoco2 { Data = 3 },
					new SimplePoco2 { Data = 4 }
				};

				await provider.ExecuteAsync(
					schema.CreateTable<SimplePoco1>(),
					schema.CreateTable<SimplePoco2>(),
					relationship.CreateTable(),
					schema.CreateInsert(inPoco1s),
					schema.CreateInsert(inPoco2s),
					relationship.CreateInsert(inPoco1s[0], inPoco2s),
					relationship.CreateInsert(inPoco1s[1], inPoco2s)
					);

				using (var result = await provider.ExecuteReaderAsync(QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(tableName))))
				{
					Assert.IsTrue(result.HasRows);

					var outPairs = new List<(Guid, Guid)>();
					while (await result.ReadAsync())
					{
						outPairs.Add(
							(result.GetGuid(0), result.GetGuid(1))
							);
					}

					Assert.AreEqual(4, outPairs.Count);
				}

				await provider.ExecuteAsync(relationship.CreateDelete(inPoco2s[0]));

				using (var result = await provider.ExecuteReaderAsync(QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(tableName))))
				{
					Assert.IsTrue(result.HasRows);

					var outPairs = new List<(Guid, Guid)>();
					while (await result.ReadAsync())
					{
						outPairs.Add(
							(result.GetGuid(0), result.GetGuid(1))
							);
					}

					Assert.AreEqual(2, outPairs.Count);
				}
			}
		}

		[TestMethod]
		public async Task DeleteSelectiveRightToLeftRelationships()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<SimplePoco1>();
			schemaBuilder.DefineEntity<SimplePoco2>();
			schemaBuilder.DefineRelationship<SimplePoco1, SimplePoco2>("Relationship");
			var schema = schemaBuilder.Build();
			var relationship = schema.GetRelationship<SimplePoco1, SimplePoco2>("Relationship");
			var tableName = relationship.JunctionTable.TableName;

			using (var provider = TestHelper.CreateProvider())
			{
				var inPoco1s = new[]
				{
					new SimplePoco1 { Data = 1 },
					new SimplePoco1 { Data = 2 },
					new SimplePoco1 { Data = 3 }
				};
				var inPoco2s = new[]
				{
					new SimplePoco2 { Data = 4 },
					new SimplePoco2 { Data = 5 },
					new SimplePoco2 { Data = 6 }
				};

				await provider.ExecuteAsync(
					schema.CreateTable<SimplePoco1>(),
					schema.CreateTable<SimplePoco2>(),
					relationship.CreateTable(),
					schema.CreateInsert(inPoco1s),
					schema.CreateInsert(inPoco2s),
					relationship.CreateInsert(inPoco1s[0], inPoco2s),
					relationship.CreateInsert(inPoco1s[1], inPoco2s),
					relationship.CreateInsert(inPoco1s[2], inPoco2s)
					);

				using (var result = await provider.ExecuteReaderAsync(QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(tableName))))
				{
					Assert.IsTrue(result.HasRows);

					var outPairs = new List<(Guid, Guid)>();
					while (await result.ReadAsync())
					{
						outPairs.Add(
							(result.GetGuid(0), result.GetGuid(1))
							);
					}

					Assert.AreEqual(9, outPairs.Count);
				}

				await provider.ExecuteAsync(relationship.CreateDelete(inPoco2s[0], inPoco1s[0], inPoco1s[1]));

				using (var result = await provider.ExecuteReaderAsync(QueryExpression.Select(QueryExpression.All(), QueryExpression.Table(tableName))))
				{
					Assert.IsTrue(result.HasRows);

					var outPairs = new List<(Guid, Guid)>();
					while (await result.ReadAsync())
					{
						outPairs.Add(
							(result.GetGuid(0), result.GetGuid(1))
							);
					}

					Assert.AreEqual(7, outPairs.Count);
				}
			}
		}

		private class SimplePoco1
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}

		private class SimplePoco2
		{
			public Guid Id { get; private set; }
			public int Data { get; set; }
		}
	}
}
