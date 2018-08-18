using System;
using System.Collections.Generic;
using System.Text;
using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Operations;
using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Queries
{
	public class ModelledProjection<T> : IProjectionMapping<T>
	{
		private static readonly string[] _self = new[] { "." };

		private readonly IProjectionModel _projectionModel;

		public ModelledProjection(IProjectionModel projectionModel)
		{
			_projectionModel = projectionModel;
		}

		public IModelReadWriter CreateReader(QueryResult queryResult)
		{
			return new QueryResultReader(_projectionModel, queryResult);
		}

		public void Inject(T obj, IModelReadWriter readWriter)
		{
			var objectReadWriter = new ObjectReadWriter(obj, TypeModel.GetModelOf<T>(), typeof(T));
			_projectionModel.Mapping.PerformMapping(readWriter, objectReadWriter);
		}

		public T Map(IModelReadWriter readWriter)
		{
			var objectReadWriter = new ObjectReadWriter(null, TypeModel.GetModelOf<T>(), typeof(T));
			_projectionModel.Mapping.PerformMapping(readWriter, objectReadWriter);
			return objectReadWriter.ReadField<T>(_self, 0);
		}
	}
}
