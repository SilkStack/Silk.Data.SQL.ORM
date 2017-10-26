using System;

namespace Silk.Data.SQL.ORM
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DataLengthAttribute : Attribute
	{
		public DataLengthAttribute(int dataLength)
		{
			DataLength = dataLength;
		}

		public int DataLength { get; }
	}
}
