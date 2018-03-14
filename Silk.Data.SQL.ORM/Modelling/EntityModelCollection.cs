using System.Collections;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class EntityModelCollection : IEnumerable<EntityModel>
	{
		private readonly List<EntityModel> _entityModels =
			new List<EntityModel>();

		public EntityModelCollection(IEnumerable<EntityModel> entityModels)
		{
			_entityModels.AddRange(entityModels);
		}

		public IEnumerator<EntityModel> GetEnumerator()
		{
			return _entityModels.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
