using System;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class DataField : IViewField
	{
		public string Name { get; }

		public Type DataType { get; }

		public object[] Metadata { get; }

		public ModelBinding ModelBinding { get; }

		public DataField(string name, Type dataType, object[] metadata,
			ModelBinding modelBinding)
		{
			Name = name;
			DataType = dataType;
			Metadata = metadata;
			ModelBinding = modelBinding;
		}

		public static DataField FromDefinition(ViewFieldDefinition definition)
		{
			return new DataField(definition.Name, definition.DataType, definition.Metadata.ToArray(),
				definition.ModelBinding);
		}

		public static IEnumerable<DataField> FromDefinitions(IEnumerable<ViewFieldDefinition> definitions)
		{
			return definitions.Select(q => FromDefinition(q));
		}
	}
}
