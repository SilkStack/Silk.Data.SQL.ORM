using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.ORM.Modelling.Binding;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Modelling
{
	public abstract class EntityModel : ModelBase<IEntityField>, IProjectionModel
	{
		private readonly Dictionary<Type, IProjectionModel> _projectionCache
			= new Dictionary<Type, IProjectionModel>();

		public Table EntityTable { get; }
		public Mapping Mapping { get; protected set; }
		public override IEntityField[] Fields { get; }
		public Type EntityType { get; }
		public Table[] JunctionTables { get; protected set; }

		public EntityModel(Type entityType, IEntityField[] fields, Table entityTable)
		{
			EntityType = entityType;
			Fields = fields;
			EntityTable = entityTable;
		}

		public IProjectionModel GetProjection<TProjection>()
		{
			var type = typeof(TProjection);
			if (_projectionCache.TryGetValue(type, out var projection))
				return projection;

			lock (_projectionCache)
			{
				if (_projectionCache.TryGetValue(type, out projection))
					return projection;

				var transformer = new ProjectionModelTransformer<TProjection>(type);
				Transform(transformer);
				projection = transformer.GetProjectionModel();
				_projectionCache.Add(type, projection);

				return projection;
			}
		}

		public abstract AssignmentBinding GetCreateInstanceAsNeededBinding(string[] path);
	}

	public class EntityModel<T> : EntityModel, IModelBuilderFinalizer
	{
		public EntityModel(IEntityField[] fields, Table entityTable) :
			base(typeof(T), fields, entityTable)
		{
		}

		public void FinalizeBuiltModel(Schema.Schema finalizingSchema, List<Table> tables)
		{
			var mappingBuilder = new MappingBuilder(this, TypeModel.GetModelOf<T>());
			mappingBuilder.AddConvention(CreateInstanceAsNeeded.Instance);
			mappingBuilder.AddConvention(CreateEmbeddedInstanceUsingNotNullColumn.Instance);
			mappingBuilder.AddConvention(CreateSingleRelatedInstanceWhenPresent.Instance);
			mappingBuilder.AddConvention(CopyValueFields.Instance);

			Mapping = mappingBuilder.BuildMapping();

			JunctionTables = Fields.OfType<IManyRelatedObjectField>()
				.Select(q => q.JunctionTable).ToArray();
		}

		public override AssignmentBinding GetCreateInstanceAsNeededBinding(string[] path)
		{
			return new CreateInstanceIfNull<T>(GetConstructor(typeof(T)), path);
		}

		private static ConstructorInfo GetConstructor(Type type)
		{
			var ctor = type.GetTypeInfo().DeclaredConstructors
				.FirstOrDefault(q => q.GetParameters().Length == 0);
			if (ctor == null)
			{
				throw new MappingRequirementException($"A constructor with 0 parameters is required on type {type}.");
			}
			return ctor;
		}
	}
}
