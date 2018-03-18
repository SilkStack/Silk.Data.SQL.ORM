using Silk.Data.SQL.Queries;
using System.Linq;
using System.Text;

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
			var output = new StringBuilder();
			for (var i = 0; i < words.Length; i++)
			{
				var word = words[i];
				if (i > 0)
				{
					if (word == "SELECT" || word == "FROM" || word == "WHERE" || words.Skip(i).Take(3).SequenceEqual(new[] { "LEFT", "OUTER", "JOIN" }))
						output.AppendLine();
					else
						output.Append(" ");
				}
				output.Append(word);
			}
			return output.ToString();
		}
	}
}
