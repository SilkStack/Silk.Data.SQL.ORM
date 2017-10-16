using System;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class DataField : IViewField
	{
		public string Name { get; }

		public Type DataType { get; }

		public object[] Metadata { get; }

		public ModelBinding ModelBinding { get; }
	}
}
