namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Specifies how a primary key is generated.
	/// </summary>
	public enum PrimaryKeyGenerator
	{
		/// <summary>
		/// Field is not a primary key.
		/// </summary>
		NotPrimaryKey,
		/// <summary>
		/// The SQL server will auto-generate the primary key value.
		/// </summary>
		ServerGenerated,
		/// <summary>
		/// The client will generate or use the assigned primary key value.
		/// </summary>
		ClientGenerated
	}
}
