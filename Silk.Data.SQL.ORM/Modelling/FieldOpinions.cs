namespace Silk.Data.SQL.ORM.Modelling
{
	/// <summary>
	/// Describes developer opinions on how to model a field.
	/// </summary>
	public class FieldOpinions
	{
		/// <summary>
		/// Gets a default <see cref="FieldOpinions"/> instance.
		/// </summary>
		public static FieldOpinions Default { get; } = new FieldOpinions();

		public SqlDataType DataType { get; }
		public int? DataLength { get; }
		public bool IsPrimaryKey { get; }
		public bool AutoGenerate { get; }
		public bool IsIndex { get; }
		public bool IsUnique { get; }
		public int? Precision { get; }
		public int? Scale { get; }
		public string Name { get; }

		private FieldOpinions() { }

		public FieldOpinions(SqlDataType dataType, int? dataLength,
			bool isPrimaryKey, bool autoGenerate,
			bool isIndex, bool isUnique,
			int? precision, int? scale,
			string name)
		{
			DataType = dataType;
			DataLength = dataLength;
			IsPrimaryKey = isPrimaryKey;
			AutoGenerate = autoGenerate;
			IsIndex = isIndex;
			IsUnique = isUnique;
			Precision = precision;
			Scale = scale;
			Name = name;
		}
	}
}
