using Silk.Data.Modelling;
using Silk.Data.Modelling.GenericDispatch;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Schema for storing and querying an entity type.
	/// </summary>
	public abstract class EntitySchema : IModel<SchemaField>
	{
		public Schema Schema { get; internal set; }

		public abstract Type EntityType { get; }
		public abstract Table EntityTable { get; }
		public abstract SchemaIndex[] Indexes { get; }
		public IMapping<EntitySchema, SchemaField, TypeModel, PropertyInfoField> Mapping { get; protected set; }

		public IReadOnlyList<SchemaField> Fields { get; }

		IReadOnlyList<IField> IModel.Fields => Fields;

		public EntitySchema(SchemaField[] schemaFields)
		{
			Fields = schemaFields;
		}

		public IEnumerable<SchemaField> GetPathFields(IFieldPath<SchemaField> fieldPath)
		{
			throw new NotImplementedException();
		}

		public void Dispatch(IModelGenericExecutor executor)
		{
			throw new NotImplementedException();
		}
	}

	public class ProjectionSchema<TProjection, TEntity> : EntitySchema
		where TEntity : class
		where TProjection : class
	{
		public override Table EntityTable { get; }
		public override Type EntityType => typeof(TEntity);
		public override SchemaIndex[] Indexes { get; }

		public new SchemaField<TEntity>[] Fields { get; }

		public ProjectionSchema(Table entityTable, SchemaField<TEntity>[] schemaFields,
			SchemaIndex[] indexes, IMapping<EntitySchema, SchemaField, TypeModel, PropertyInfoField> mapping)
			: base(schemaFields)
		{
			EntityTable = entityTable;
			Indexes = indexes;
			Fields = schemaFields;
			Mapping = mapping;
		}
	}

	/// <summary>
	/// Schema for storing and querying entities of type T.
	/// </summary>
	public class EntitySchema<T> : ProjectionSchema<T, T>
		where T : class
	{
		private readonly Dictionary<Type, EntitySchema> _projectionCache
			= new Dictionary<Type, EntitySchema>();

		public SchemaModel SchemaModel { get; }

		public EntitySchema(Table entityTable, SchemaField<T>[] schemaFields, SchemaIndex[] indexes) :
			base(entityTable, schemaFields, indexes, null)
		{
			SchemaModel = SchemaModel.Create(this);
			//AssignFieldAndModelPropertiesOnSchemaFieldReferences();
			//Mapping = new Mapping(
			//	TypeModel.GetModelOf<T>(),
			//	null,
			//	new Modelling.Mapping.Binding.Binding[]
			//	{
			//		new CreateInstanceIfNull<T>(
			//			SqlTypeHelper.GetConstructor(typeof(T)), TypeModel.GetModelOf<T>().Root
			//			)
			//	}.Concat(schemaFields
			//		.Where(q => q.SchemaFieldReference.Field.CanWrite)
			//		.SelectMany(q => q.Bindings)).ToArray());
		}

		//private void AssignFieldAndModelPropertiesOnSchemaFieldReferences()
		//{
		//	foreach (var schemaField in Fields)
		//	{
		//		var schemaFieldReference = schemaField.SchemaFieldReference as FieldReferenceBase;
		//		schemaFieldReference.Model = SchemaModel;
		//		schemaFieldReference.Field = SchemaModel.GetField(schemaField.ModelPath);
		//	}
		//}

		public ProjectionSchema<TProjection, T> GetProjection<TProjection>()
			where TProjection : class
		{
			if (_projectionCache.TryGetValue(typeof(TProjection), out var projection))
				return projection as ProjectionSchema<TProjection, T>;

			lock (_projectionCache)
			{
				var projectionBuilder = new ProjectionBuilder<T, TProjection>();
				projection = projectionBuilder.Build(this);
				_projectionCache.Add(typeof(TProjection), projection);
				return projection as ProjectionSchema<TProjection, T>;
			}
		}
	}
}
