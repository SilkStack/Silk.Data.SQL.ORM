using System;

namespace Silk.Data.SQL.ORM
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class IsNullableAttribute : Attribute
	{
		public bool IsNullable { get; }

		public IsNullableAttribute(bool isNullable)
		{
			IsNullable = isNullable;
		}
	}
}
