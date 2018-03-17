using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Tests
{
	public class TestQueryConverter : QueryConverterCommonBase
	{
		protected override string ProviderName => "tests";

		protected override string AutoIncrementSql => "AUTOINC";

		protected override string GetDbDatatype(SqlDataType sqlDataType)
		{
			return $"{sqlDataType.BaseType}({string.Join(",", sqlDataType.Parameters)})";
		}

		protected override string QuoteIdentifier(string schemaComponent)
		{
			if (schemaComponent == "*")
				return "*";
			return $"[{schemaComponent}]";
		}

		public static string CleanSql(string sqlQuery)
		{
			var words = sqlQuery.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
			return string.Join(' ', words);
		}
	}
}
