using System;

namespace Silk.Data.SQL.ORM
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class AutoIncrementAttribute : Attribute
	{
	}
}
