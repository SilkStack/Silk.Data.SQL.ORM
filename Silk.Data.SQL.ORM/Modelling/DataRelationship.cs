using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class DataRelationship
    {
		private readonly Lazy<DataField> _lazyForeignField;
		private readonly Lazy<EntityModel> _lazyForeignModel;
		private readonly RelationshipDefinition _relationshipDefinition;
		private readonly DataDomain _dataDomain;

		public DataField ForeignField => _lazyForeignField.Value;
		public EntityModel ForeignModel => _lazyForeignModel.Value;

		public DataRelationship(RelationshipDefinition relationshipDefinition, DataDomain domain)
		{
			_relationshipDefinition = relationshipDefinition;
			_dataDomain = domain;

			_lazyForeignField = new Lazy<DataField>(() =>
			{
				return relationshipDefinition.Domain.DataModels
					.FirstOrDefault(q => q.EntityType == _relationshipDefinition.EntityType)
					?.Fields.FirstOrDefault(q => q.Name == _relationshipDefinition.RelationshipField);
			});

			_lazyForeignModel = new Lazy<EntityModel>(() =>
			{
				var dataModel = _relationshipDefinition.Domain.DataModels
					.FirstOrDefault(q => q.EntityType == _relationshipDefinition.EntityType);
				if (_relationshipDefinition.EntityType != _relationshipDefinition.ProjectionType)
				{
					dataModel = dataModel.GetType()
						.GetMethod(nameof(EntityModel<object>.GetSubView))
						.MakeGenericMethod(_relationshipDefinition.ProjectionType)
						.Invoke(dataModel, new object[0]) as EntityModel;
				}
				return dataModel;
			});
		}
    }
}
