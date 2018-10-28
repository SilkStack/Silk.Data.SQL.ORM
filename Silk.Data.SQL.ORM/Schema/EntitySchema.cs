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
		public Mapping Mapping { get; private set; }

		public IEntityField[] EntityFields { get; }

		public EntitySchema(IEntityField[] entityFields)
		{
			EntityFields = entityFields;
		}

		protected void CreateMapping()
		{
			Mapping = new Mapping(TypeModel.GetModelOf(EntityType), null,
				CreateMappingBindings().ToArray());
		}

		protected abstract IEnumerable<Modelling.Mapping.Binding.Binding> CreateMappingBindings();
	}

	public class ProjectionSchema<T> : EntitySchema
	{
		public override Table EntityTable { get; }
		public override Type EntityType { get; }
		public override SchemaIndex[] Indexes { get; }
		public override ProjectionField[] ProjectionFields { get; }
		public override EntityFieldJoin[] EntityJoins { get; }
		private Mapping _entityToViewTypeMapping { get; }
		private MappingVisitor _mappingVisitor;

		public ProjectionSchema(Table entityTable, IEntityField[] entityFields,
			ProjectionField[] projectionFields, EntityFieldJoin[] manyToOneJoins,
			SchemaIndex[] indexes, Type entityType, Mapping entityToViewTypeMapping) : base(entityFields)
		{
			EntityTable = entityTable;
			ProjectionFields = projectionFields;
			EntityJoins = manyToOneJoins;
			Indexes = indexes;
			EntityType = entityType;
			_entityToViewTypeMapping = entityToViewTypeMapping;

			_mappingVisitor = new MappingVisitor(projectionFields.OfType<MappedProjectionField>().ToArray());

			CreateMapping();
		}

		protected override IEnumerable<Modelling.Mapping.Binding.Binding> CreateMappingBindings()
		{
			return _mappingVisitor.Visit(_entityToViewTypeMapping);
		}
	}

	/// <summary>
	/// Schema for storing and querying entities of type T.
	/// </summary>
	public class EntitySchema<T> : ProjectionSchema<T>
	{
		private readonly Dictionary<Type, EntitySchema> _projectionCache
			= new Dictionary<Type, EntitySchema>();

		public new IEntityFieldOfEntity<T>[] EntityFields { get; }

		public EntitySchema(Table entityTable, IEntityFieldOfEntity<T>[] entityFields,
			ProjectionField[] projectionFields, EntityFieldJoin[] manyToOneJoins,
			SchemaIndex[] indexes) : base(entityTable, entityFields, projectionFields,
				manyToOneJoins, indexes, typeof(T), null)
		{
			EntityFields = entityFields;
			CreateMapping();
		}

		protected override IEnumerable<Modelling.Mapping.Binding.Binding> CreateMappingBindings()
		{
			yield return new CreateInstanceIfNull<T>(SqlTypeHelper.GetConstructor(typeof(T)), new[] { "." });
			foreach (var field in ProjectionFields)
			{
				yield return field.GetMappingBinding("");
			}
		}

		public ProjectionSchema<TProjection> GetProjection<TProjection>()
			where TProjection : class
		{
			if (_projectionCache.TryGetValue(typeof(TProjection), out var projection))
				return projection as ProjectionSchema<TProjection>;

			var projections = default(List<ProjectionField>);
			lock (_projectionCache)
			{
				var mapping = GetMapping(EntityType, typeof(TProjection), Schema.ProjectionMappingOptions);
				projections = new List<ProjectionField>();

				GetProjections(mapping.Bindings);

				projection = new ProjectionSchema<TProjection>(
					EntityTable, EntityFields, projections.ToArray(),
					EntityJoins, Indexes, EntityType, mapping
					);
				_projectionCache.Add(typeof(TProjection), projection);
				return projection as ProjectionSchema<TProjection>;
			}

			void GetProjections(Modelling.Mapping.Binding.Binding[] bindings, string[] toPath = null,
				string[] fromPath = null)
			{
				if (toPath == null)
					toPath = new string[0];
				if (fromPath == null)
					fromPath = new string[0];

				foreach (var binding in bindings)
				{
					if (binding is SubmappingBindingBase submappingBinding)
					{
						GetProjections(
							submappingBinding.Mapping.Bindings,
							toPath.Concat(submappingBinding.ToPath).ToArray(),
							fromPath.Concat(submappingBinding.FromPath).ToArray()
							);
					}
					else if (binding is MappingBinding mappingBinding)
					{
						var mappingFromPath = fromPath.Concat(mappingBinding.FromPath);
						var mappingToPath = toPath.Concat(mappingBinding.ToPath).ToArray();

						var sourceProjection = ProjectionFields.FirstOrDefault(q => q.ModelPath.SequenceEqual(mappingFromPath));
						if (sourceProjection == null)
							continue;

						projections.Add(new MappedProjectionField(
							sourceProjection.SourceName, sourceProjection.FieldName, sourceProjection.AliasName,
							mappingToPath, sourceProjection.Join, sourceProjection.IsNullCheck, sourceProjection,
							mappingBinding
							));
					}
				}
			}
		}

		private readonly static object _syncObject = new object();
		private readonly static MappingStore _mappingStore = new MappingStore();

		private static Silk.Data.Modelling.Mapping.Mapping GetMapping(Type fromType, Type toType,
			MappingOptions options)
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
				foreach (var convention in options.Conventions)
				{
					mappingBuilder.AddConvention(convention);
				}
				return mappingBuilder.BuildMapping();
			}
		}
	}

	internal class MappingVisitor
	{
		private readonly MappedProjectionField[] _projectionFields;

		public MappingVisitor(MappedProjectionField[] projectionFields)
		{
			_projectionFields = projectionFields;
		}

		public IEnumerable<Modelling.Mapping.Binding.Binding> Visit(Mapping mapping, string[] path = null)
		{
			if (path == null)
				path = new string[0];

			foreach (var binding in mapping.Bindings)
			{
				if (binding is AssignmentBinding assignmentBinding)
				{
					var projectionBinding = VisitAssignmentBinding(assignmentBinding, path);
					if (projectionBinding != null)
						yield return projectionBinding;
				}
				else if (binding is SubmappingBindingBase submappingBinding)
				{
					foreach (var subBinding in VisitSubmappingBinding(submappingBinding, path))
						yield return subBinding;
				}
				else if (binding is MappingBinding mappingBinding)
				{
					var projectionBinding = VisitMappingBinding(mappingBinding, path);
					if (projectionBinding != null)
						yield return projectionBinding;
				}
			}
		}

		public Modelling.Mapping.Binding.Binding VisitAssignmentBinding(AssignmentBinding assignmentBinding, string[] path)
		{
			return new MappedBinding(null, assignmentBinding, path);
		}

		public IEnumerable<Modelling.Mapping.Binding.Binding> VisitSubmappingBinding(SubmappingBindingBase submappingBinding, string[] path)
		{
			return Visit(submappingBinding.Mapping, path.Concat(submappingBinding.ToPath).ToArray());
		}

		public Modelling.Mapping.Binding.Binding VisitMappingBinding(MappingBinding mappingBinding, string[] path)
		{
			var sourceProjection = _projectionFields.FirstOrDefault(
				q => q.ModelPath.SequenceEqual(path.Concat(mappingBinding.ToPath))
				);
			if (sourceProjection == null)
				return null;

			return new MappedBinding(sourceProjection.GetMappingBinding(""), mappingBinding, path);
		}

		private class MappedBinding : Modelling.Mapping.Binding.Binding
		{
			private readonly Modelling.Mapping.Binding.Binding _sourceBinding;
			private readonly Modelling.Mapping.Binding.Binding _mappingBinding;
			private readonly string[] _path;

			public MappedBinding(Modelling.Mapping.Binding.Binding sourceBinding, Modelling.Mapping.Binding.Binding mappingBinding,
				string[] path)
			{
				_sourceBinding = sourceBinding;
				_mappingBinding = mappingBinding;
				_path = path;
			}

			public override void PerformBinding(IModelReadWriter from, IModelReadWriter to)
			{
				var proxyReadWriter = _path.Length < 1 ? to : new SubmappingModelReadWriter(to, to.Model, _path);
				if (_sourceBinding != null)
				{
					var mappedReadWriter = new MappedReadWriter();
					_sourceBinding.PerformBinding(from, mappedReadWriter);
					_mappingBinding.PerformBinding(mappedReadWriter, proxyReadWriter);
				}
				else
				{
					_mappingBinding.PerformBinding(from, proxyReadWriter);
				}
			}
		}

		private class SubmappingModelReadWriter : IModelReadWriter
		{
			public IModel Model { get; }
			public IModelReadWriter RealReadWriter { get; }
			public string[] PrefixPath { get; }

			public SubmappingModelReadWriter(IModelReadWriter modelReadWriter, IModel model,
				string[] prefixPath)
			{
				Model = model;
				RealReadWriter = modelReadWriter;
				PrefixPath = prefixPath;
			}

			public T ReadField<T>(Span<string> path)
			{
				var fixedPath = PrefixPath.Concat(path.ToArray()).ToArray();
				return RealReadWriter.ReadField<T>(fixedPath);
			}

			public void WriteField<T>(Span<string> path, T value)
			{
				var fixedPath = PrefixPath.Concat(path.ToArray()).ToArray();
				RealReadWriter.WriteField<T>(fixedPath, value);
			}
		}

		private class MappedReadWriter : IModelReadWriter
		{
			private object _value;

			public IModel Model => throw new System.NotImplementedException();

			public T ReadField<T>(Span<string> path)
			{
				return (T)_value;
			}

			public void WriteField<T>(Span<string> path, T value)
			{
				_value = value;
			}
		}
	}
}
