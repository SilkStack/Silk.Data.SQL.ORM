using System.Collections.Generic;
using System.Linq;
using Silk.Data.Modelling;
using Silk.Data.Modelling.GenericDispatch;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Model of an entity type.
	/// </summary>
	public abstract class EntityModel : IModel<EntityField>
	{
		public TableSchema TableSchema { get; }

		public string TableName { get; }

		public IReadOnlyList<EntityField> Fields { get; }
		IReadOnlyList<IField> IModel.Fields => Fields;

		public abstract TypeModel TypeModel { get; }

		protected EntityModel(IEnumerable<EntityField> entityFields,
			string tableName)
		{
			Fields = entityFields.ToArray();
			TableSchema = new TableSchema(Fields.SelectMany(q => q.Columns));
			TableName = tableName;
		}

		public abstract void Dispatch(IModelGenericExecutor executor);

		public IEnumerable<EntityField> GetPathFields(IFieldPath<EntityField> fieldPath)
			=> fieldPath.FinalField.SubFields;
	}

	/// <summary>
	/// Model of an entity type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EntityModel<T> : EntityModel
		where T : class
	{
		public override TypeModel TypeModel { get; } = TypeModel.GetModelOf<T>();

		public EntityModel(IEnumerable<EntityField> entityFields) :
			base(entityFields, /*todo: temporary until table name provided by entity config*/ typeof(T).Name)
		{
		}

		public override void Dispatch(IModelGenericExecutor executor)
			=> executor.Execute<EntityModel, EntityField, T>(this);
	}
}
