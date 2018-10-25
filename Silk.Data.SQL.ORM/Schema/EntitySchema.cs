using Silk.Data.Modelling;
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
	public abstract class EntitySchema
	{
		public Schema Schema { get; internal set; }

		public abstract Type EntityType { get; }
		public abstract Table EntityTable { get; }
		public abstract SchemaIndex[] Indexes { get; }
		public abstract ProjectionField[] ProjectionFields { get; }
		public abstract EntityFieldJoin[] EntityJoins { get; }

		public IEntityField[] EntityFields { get; }

		public EntitySchema(IEntityField[] entityFields)
		{
			EntityFields = entityFields;
		}

		public abstract IEnumerable<Modelling.Mapping.Binding.Binding> CreateMappingBindings(string aliasPrefix);
	}

	public class ProjectionSchema<T> : EntitySchema
	{
		public override Table EntityTable { get; }
		public override Type EntityType { get; }
		public override SchemaIndex[] Indexes { get; }
		public override ProjectionField[] ProjectionFields { get; }
		public override EntityFieldJoin[] EntityJoins { get; }
		public Mapping Mapping { get; }
		private MappingVisitor _mappingVisitor = new MappingVisitor();

		public ProjectionSchema(Table entityTable, IEntityField[] entityFields,
			ProjectionField[] projectionFields, EntityFieldJoin[] manyToOneJoins,
			SchemaIndex[] indexes, Type entityType, Mapping mapping) : base(entityFields)
		{
			EntityTable = entityTable;
			ProjectionFields = projectionFields;
			EntityJoins = manyToOneJoins;
			Indexes = indexes;
			EntityType = entityType;
			Mapping = mapping;
		}

		public override IEnumerable<Modelling.Mapping.Binding.Binding> CreateMappingBindings(string aliasPrefix)
		{
			return _mappingVisitor.Visit(Mapping, aliasPrefix);
			//yield return new CreateInstanceIfNull<T>(SqlTypeHelper.GetConstructor(typeof(T)), new[] { "." });
			//foreach (var field in ProjectionFields)
			//{
			//	yield return field.GetMappingBinding(aliasPrefix);
			//}
		}
	}

	/// <summary>
	/// Schema for storing and querying entities of type T.
	/// </summary>
	public class EntitySchema<T> : ProjectionSchema<T>
	{
		public new IEntityFieldOfEntity<T>[] EntityFields { get; }

		public EntitySchema(Table entityTable, IEntityFieldOfEntity<T>[] entityFields,
			ProjectionField[] projectionFields, EntityFieldJoin[] manyToOneJoins,
			SchemaIndex[] indexes) : base(entityTable, entityFields, projectionFields,
				manyToOneJoins, indexes, typeof(T), null)
		{
			EntityFields = entityFields;
		}

		public override IEnumerable<Modelling.Mapping.Binding.Binding> CreateMappingBindings(string aliasPrefix)
		{
			yield return new CreateInstanceIfNull<T>(SqlTypeHelper.GetConstructor(typeof(T)), new[] { "." });
			foreach (var field in ProjectionFields)
			{
				yield return field.GetMappingBinding(aliasPrefix);
			}
		}

		public ProjectionSchema<TProjection> GetProjection<TProjection>()
			where TProjection : class
		{
			var mapping = GetMapping(EntityType, typeof(TProjection));
			var projections = new List<ProjectionField>();

			foreach (var binding in mapping.Bindings)
			{
				if (binding is AssignmentBinding assignmentBinding)
				{

				}
				else if (binding is SubmappingBindingBase submappingBinding)
				{

				}
				else if (binding is MappingBinding mappingBinding)
				{
					var fromPath = mappingBinding.FromPath;
					var toPath = mappingBinding.ToPath;

					var sourceProjection = ProjectionFields.FirstOrDefault(q => q.ModelPath.SequenceEqual(fromPath));
					if (sourceProjection == null)
						continue;

					projections.Add(new MappedProjectionField(
						sourceProjection.SourceName, sourceProjection.FieldName, sourceProjection.AliasName,
						toPath, sourceProjection.Join, sourceProjection.IsNullCheck, sourceProjection,
						mappingBinding
						));
				}
			}

			return new ProjectionSchema<TProjection>(
				EntityTable, EntityFields, projections.ToArray(),
				EntityJoins, Indexes, EntityType, mapping
				);
		}

		private readonly static object _syncObject = new object();
		private readonly static MappingStore _mappingStore = new MappingStore();
		private readonly static MappingOptions _options = MappingOptions.DefaultObjectMappingOptions;

		private static Silk.Data.Modelling.Mapping.Mapping GetMapping(Type fromType, Type toType)
		{
			var fromModel = TypeModel.GetModelOf(fromType);
			var toModel = TypeModel.GetModelOf(toType);
			if (_mappingStore.TryGetMapping(fromModel, toModel, out var mapping))
				return mapping;

			lock (_syncObject)
			{
				if (_mappingStore.TryGetMapping(fromModel, toModel, out mapping))
					return mapping;

				var mappingBuilder = new MappingBuilder(fromModel, toModel, _mappingStore);
				foreach (var convention in _options.Conventions)
				{
					mappingBuilder.AddConvention(convention);
				}
				return mappingBuilder.BuildMapping();
			}
		}
	}

	internal class MappingVisitor
	{
		public IEnumerable<Modelling.Mapping.Binding.Binding> Visit(Mapping mapping, string prefix)
		{
			foreach (var binding in mapping.Bindings)
			{
				if (binding is AssignmentBinding assignmentBinding)
				{
					yield return VisitAssignmentBinding(assignmentBinding);
				}
				else if (binding is SubmappingBindingBase submappingBinding)
				{
					foreach (var subBinding in VisitSubmappingBinding(submappingBinding))
						yield return subBinding;
				}
				else if (binding is MappingBinding mappingBinding)
				{
					yield return VisitMappingBinding(mappingBinding);
				}
			}
		}

		public Modelling.Mapping.Binding.Binding VisitAssignmentBinding(AssignmentBinding assignmentBinding)
		{
			return null;
		}

		public IEnumerable<Modelling.Mapping.Binding.Binding> VisitSubmappingBinding(SubmappingBindingBase submappingBinding)
		{
			yield break;
		}

		public Modelling.Mapping.Binding.Binding VisitMappingBinding(MappingBinding mappingBinding)
		{
			return null;
		}
	}
}
