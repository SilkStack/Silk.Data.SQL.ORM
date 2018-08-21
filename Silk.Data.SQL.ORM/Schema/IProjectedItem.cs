using System;
using System.Collections.Generic;
using System.Text;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// An item projected in a SELECT query.
	/// </summary>
	public interface IProjectedItem
	{
		/// <summary>
		/// Gets the table name/alias that the field is a member of.
		/// </summary>
		string SourceName { get; }
		/// <summary>
		/// Gets the name of the field.
		/// </summary>
		string FieldName { get; }
		/// <summary>
		/// Gets the alias the field should be projected as.
		/// </summary>
		string AliasName { get; }
	}
}
