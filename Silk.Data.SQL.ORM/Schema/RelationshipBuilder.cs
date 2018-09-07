using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class RelationshipBuilder
	{
		public Type Left { get; }
		public Type Right { get; }
		public string Name { get; set; }

		public RelationshipBuilder(Type left, Type right)
		{
			Left = left;
			Right = right;
		}

		public abstract Relationship Build(PartialEntitySchemaCollection partialEntities);
	}

	public class RelationshipBuilder<TLeft, TRight> : RelationshipBuilder
		where TLeft : class
		where TRight : class
	{
		public RelationshipBuilder() : base(typeof(TLeft), typeof(TRight)) { }

		public override Relationship Build(PartialEntitySchemaCollection partialEntities)
		{
			if (!partialEntities.IsEntityTypeDefined(typeof(TLeft)) ||
				!partialEntities.IsEntityTypeDefined(typeof(TRight)))
				throw new Exception("Related entity types not registered in schema.");

			var leftPartialSchema = partialEntities[typeof(TLeft)];
			var rightPartialSchema = partialEntities[typeof(TRight)];

			var leftRelationship = leftPartialSchema.CreateRelatedEntityField<TLeft>(
				"Left", typeof(TLeft),
				null, partialEntities, leftPartialSchema.TableName, new[] { "." }
				);
			var rightRelationship = rightPartialSchema.CreateRelatedEntityField<TRight>(
				"Right", typeof(TRight),
				null, partialEntities, rightPartialSchema.TableName, new[] { "." }
				);

			var table = new Table(Name, leftRelationship.Columns.Concat(rightRelationship.Columns).ToArray());

			return new Relationship<TLeft, TRight>(Name, table, leftRelationship, rightRelationship);
		}
	}
}
