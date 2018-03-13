using Silk.Data.Modelling;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class EntityModelTransformer : IModelTransformer
	{
		public void VisitField<T>(IField<T> field)
		{
			throw new System.NotImplementedException();
		}

		public void VisitModel<TField>(IModel<TField> model) where TField : IField
		{
			throw new System.NotImplementedException();
		}
	}
}
