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
		public Table EntityTable { get; set; }

		/// <summary>
		/// Gets a collection of relationships to single objects.
		/// </summary>
		public RelationshipCollection<SingleObjectRelationship> SingleObjectRelationships { get; set; }
		public RelationshipCollection<MultipleObjectsRelationship> MultipleObjectRelationships { get; set; }

		public EntitySchema(Model model)
		{
			Model = model;
		}

		/// <summary>
		/// Gets a projected view of the schema that can be used in queries.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public abstract Projection GetProjection(Model model);

		/// <summary>
		/// Gets a projected view of the schema that can be used in queries.
		/// </summary>
		/// <typeparam name="TProjection"></typeparam>
		/// <returns></returns>
		public abstract Projection<TProjection> GetProjection<TProjection>()
			where TProjection : new();
	}

	public class EntitySchema<T> : EntitySchema, IEntitySchema<T>
		where T : new()
	{
		public new TypedModel<T> Model { get; }

		public EntitySchema(TypedModel<T> model) : base(model)
		{
			Model = model;
		}

		public override Projection GetProjection(Model model)
		{
			throw new System.NotImplementedException();
		}

		public override Projection<TProjection> GetProjection<TProjection>()
		{
			throw new System.NotImplementedException();
		}
	}
}
