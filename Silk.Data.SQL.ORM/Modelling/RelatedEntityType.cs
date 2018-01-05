using System;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class RelatedEntityType
	{
		public Type EntityType { get; }

		public RelatedEntityType(Type relatedEntityType)
		{
			EntityType = relatedEntityType;
		}
	}
}
