using Silk.Data.Modelling;
using Silk.Data.Modelling.ResourceLoaders;

namespace Silk.Data.SQL.ORM.NewModelling
{
	//  todo: move to Modelling namespace when conflicting type is removed

	public abstract class EntitySchema : IEntitySchema
	{
		public IDataField[] Fields { get; set; }
		public string Name => Model.Name;
		public Model Model { get; }
		public IResourceLoader[] ResourceLoaders { get; set; }
		IViewField[] IView.Fields => Fields;

		/// <summary>
		/// Gets a collection of relationships to single objects.
		/// </summary>
		public RelationshipCollection<SingleObjectRelationship> SingleObjectRelationships { get; set; }
		public RelationshipCollection<MultipleObjectsRelationship> MultipleObjectRelationships { get; set; }

		public EntitySchema(Model model)
		{
			Model = model;
		}
	}

	public class EntitySchema<T> : EntitySchema, IEntitySchema<T>
		where T : new()
	{
		public new TypedModel<T> Model { get; }

		public EntitySchema(TypedModel<T> model) : base(model)
		{
			Model = model;
		}
	}
}
