using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Data schema.
	/// </summary>
	public class Schema
	{
		private readonly List<EntityModel> _entityModels =
			new List<EntityModel>();

		public Schema(IEnumerable<EntityModel> entityModels)
		{
			_entityModels.AddRange(entityModels);
		}

		/// <summary>
		/// Get all entity models for the provided type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IEnumerable<EntityModel<T>> GetAll<T>()
			where T : class
			=> _entityModels.OfType<EntityModel<T>>();
	}
}
