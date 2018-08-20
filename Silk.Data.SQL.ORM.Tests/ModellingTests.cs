using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk.Data.SQL.ORM.Schema;
using System;

namespace Silk.Data.SQL.ORM.Tests
{
	[TestClass]
	public class ModellingTests
	{
		[TestMethod]
		public void ModelPrimitivePoco()
		{
			var schemaBuilder = new SchemaBuilder();
			schemaBuilder.DefineEntity<Primitives>();

			var schema = schemaBuilder.Build();
			var entitySchema = schema.GetEntitySchema<Primitives>();

			Assert.IsNotNull(entitySchema);

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
