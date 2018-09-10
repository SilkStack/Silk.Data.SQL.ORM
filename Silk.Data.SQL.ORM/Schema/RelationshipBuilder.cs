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

			var leftJoin = new EntityFieldJoin(leftPartialSchema.TableName, $"{Name}_{leftPartialSchema.TableName}",
				Name,
				leftRelationship.Columns.Select(q => q.ColumnName).ToArray(),
				partialEntities.GetEntityPrimaryKeys<TLeft>().SelectMany(q => q.Columns).Select(q => q.ColumnName).ToArray(),
				leftRelationship, new EntityFieldJoin[0]);
			var rightJoin = new EntityFieldJoin(rightPartialSchema.TableName, $"{Name}_{rightPartialSchema.TableName}",
				Name,
				rightRelationship.Columns.Select(q => q.ColumnName).ToArray(),
				partialEntities.GetEntityPrimaryKeys<TRight>().SelectMany(q => q.Columns).Select(q => q.ColumnName).ToArray(),
				rightRelationship, new EntityFieldJoin[0]);

			return new Relationship<TLeft, TRight>(Name, table, leftRelationship, rightRelationship, leftJoin, rightJoin);
		}
	}
}
