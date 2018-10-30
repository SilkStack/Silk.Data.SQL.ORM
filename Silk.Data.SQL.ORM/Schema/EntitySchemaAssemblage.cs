using System;
using System.Collections.Generic;
using System.Text;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Represents an entity schema in the process of being assembled.
	/// </summary>
	public interface IEntitySchemaAssemblage
	{
		string TableName { get; }
	}

	/// <summary>
	/// Represents an entity schema for type T in the process of being assembled.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EntitySchemaAssemblage<T> : IEntitySchemaAssemblage
		where T : class
	{
		public string TableName { get; }

		public EntitySchemaAssemblage(string tableName)
		{
			TableName = tableName;
		}
	}
}
