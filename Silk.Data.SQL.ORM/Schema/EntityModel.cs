using System;
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
		public Table Table { get; }

		public IReadOnlyList<EntityField> Fields { get; }
		IReadOnlyList<IField> IModel.Fields => Fields;

		public abstract TypeModel TypeModel { get; }

		public abstract Type EntityType { get; }

		public IReadOnlyList<Index> Indexes { get; }

		protected EntityModel(IEnumerable<EntityField> entityFields,
			string tableName, IEnumerable<Index> indexes)
		{
			Fields = entityFields.ToArray();
			Table = new Table(tableName, Fields.SelectMany(q => q.Columns));
			Indexes = indexes?.ToArray() ?? new Index[0];
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

		public override Type EntityType => typeof(T);

		public EntityModel(IEnumerable<EntityField> entityFields, string tableName = null,
			IEnumerable<Index> indexes = null) :
			base(entityFields, tableName ?? typeof(T).Name, indexes)
		{
		}

		public override void Dispatch(IModelGenericExecutor executor)
			=> executor.Execute<EntityModel, EntityField, T>(this);
	}
}
