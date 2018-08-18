using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.ORM.Modelling.Binding;
using Silk.Data.SQL.ORM.Operations;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Modelling
{
	public interface IEntityField : IField
	{
	}

	public interface IValueField : IEntityField
	{
		Column Column { get; }

		Silk.Data.Modelling.Mapping.Binding.Binding CreateCopyBinding(string columnAlias, string[] writePath);
	}

	public interface IProjectedValueField : IValueField
	{
		string[] Path { get; }
	}

	public interface IEmbeddedObjectField : IEntityField
	{
		Column NullCheckColumn { get; }
		IEntityField[] EmbeddedFields { get; }

		AssignmentBinding CreateNullCheckBinding(string columnAlias, string[] writePath);
	}

	public interface ISingleRelatedObjectField : IEntityField
	{
		IProjectionModel RelatedObjectModel { get; }
		IProjectionModel RelatedObjectProjection { get; }
		IValueField RelatedPrimaryKey { get; }
		Column LocalColumn { get; }
	}

	public interface IManyRelatedObjectField : IEntityField
	{
		Column LocalColumn { get; }
		Table JunctionTable { get; }
		Column LocalJunctionColumn { get; }
		Column RelatedJunctionColumn { get; }
		IProjectionModel RelatedObjectModel { get; }
		IProjectionModel RelatedObjectProjection { get; }
		IValueField RelatedPrimaryKey { get; }
		TypeModel ElementModel { get; }
		Mapping Mapping { get; }
		IValueField LocalIdentifierField { get; }

		MultipleObjectMapper CreateObjectMapper(string identityFieldName);
	}

	public interface IModelBuildFinalizerField
	{
		void FinalizeModelBuild(Schema.Schema finalizingSchema, List<Table> tables);
	}

	public class ValueField<T> : FieldBase<T>, IValueField
	{
		public Column Column { get; }

		public ValueField(string fieldName, bool canRead, bool canWrite, bool isEnumerable, Type elementType,
			Column column) :
			base(fieldName, canRead, canWrite, isEnumerable, elementType)
		{
			Column = column;
		}

		public Data.Modelling.Mapping.Binding.Binding CreateCopyBinding(string columnAlias, string[] writePath)
		{
			return new CopyBinding<T>(new[] { columnAlias }, writePath);
		}
	}

	public class EmbeddedObjectField<T> : FieldBase<T>, IEmbeddedObjectField, IModelBuildFinalizerField
	{
		public Column NullCheckColumn { get; }

		private readonly ConstructorInfo _ctor;

		public IEntityField[] EmbeddedFields { get; }

		public EmbeddedObjectField(string fieldName, bool canRead, bool canWrite, bool isEnumerable, Type elementType,
			IEnumerable<IEntityField> embeddedFields, Column nullCheckColumn) :
			base(fieldName, canRead, canWrite, isEnumerable, elementType)
		{
			EmbeddedFields = embeddedFields.ToArray();
			NullCheckColumn = nullCheckColumn;
			_ctor = GetConstructor(typeof(T));
		}

		public void FinalizeModelBuild(Schema.Schema finalizingSchema, List<Table> tables)
		{
			foreach (var finalizerField in EmbeddedFields.OfType<IModelBuildFinalizerField>())
				finalizerField.FinalizeModelBuild(finalizingSchema, tables);
		}

		public AssignmentBinding CreateNullCheckBinding(string columnAlias, string[] writePath)
		{
			return new CreateEmbeddedInstanceIfPresent<T>(_ctor, writePath, columnAlias);
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

	public class SingleRelatedObjectField<T> : FieldBase<T>, ISingleRelatedObjectField, IModelBuildFinalizerField
	{
		Type IField.FieldType
		{
			get
			{
				if (RelatedObjectProjection is ViewModel viewModel)
					return viewModel.ViewType;
				return typeof(T);
			}
		}

		public IProjectionModel RelatedObjectModel { get; private set; }
		public IProjectionModel RelatedObjectProjection { get; private set; }
		public IValueField RelatedPrimaryKey { get; }
		public Column LocalColumn { get; }

		public SingleRelatedObjectField(string fieldName, bool canRead, bool canWrite, bool isEnumerable, Type elementType,
			IProjectionModel relatedObjectModel, IValueField relatedPrimaryKey, Column localColumn, IProjectionModel relatedObjectProjection) :
			base(fieldName, canRead, canWrite, isEnumerable, elementType)
		{
			RelatedObjectModel = relatedObjectModel;
			RelatedPrimaryKey = relatedPrimaryKey;
			LocalColumn = localColumn;
			RelatedObjectProjection = relatedObjectProjection;
		}

		public void FinalizeModelBuild(Schema.Schema finalizingSchema, List<Table> tables)
		{
			if (RelatedObjectModel != null)
				return;
			RelatedObjectModel = finalizingSchema.EntityModels.FirstOrDefault(
				entityModel => entityModel.Fields.Contains(RelatedPrimaryKey)
				);
			RelatedObjectProjection = RelatedObjectModel;
		}
	}

	public class ManyRelatedObjectField<T, TElement, TIdentifier> : FieldBase<T>, IManyRelatedObjectField, IModelBuildFinalizerField
		where T : class, IEnumerable<TElement>
	{
		public Column LocalColumn { get; private set; }
		public Table JunctionTable { get; private set; }
		public Column LocalJunctionColumn { get; private set; }
		public IProjectionModel RelatedObjectModel { get; private set; }
		public IValueField RelatedPrimaryKey { get; private set; }
		public Column RelatedJunctionColumn { get; private set; }
		public Mapping Mapping { get; private set; }
		public TypeModel ElementModel { get; private set; }
		public IValueField LocalIdentifierField { get; private set; }
		public IProjectionModel RelatedObjectProjection { get; private set; }

		public ManyRelatedObjectField(string fieldName, bool canRead, bool canWrite, bool isEnumerable, Type elementType,
			Column localColumn, Table junctionTable, Column localJunctionColumn, Column relatedJunctionColumn,
			IProjectionModel relatedObjectModel, IValueField relatedPrimaryKey, IValueField localIdentifierField, IProjectionModel relatedObjectProjection) :
			base(fieldName, canRead, canWrite, isEnumerable, elementType)
		{
			LocalColumn = localColumn;
			JunctionTable = junctionTable;
			LocalJunctionColumn = localJunctionColumn;
			RelatedObjectModel = relatedObjectModel;
			RelatedPrimaryKey = relatedPrimaryKey;
			RelatedJunctionColumn = relatedJunctionColumn;
			ElementModel = TypeModel.GetModelOf(elementType);
			LocalIdentifierField = localIdentifierField;
			RelatedObjectProjection = relatedObjectProjection;
		}

		public void FinalizeModelBuild(Schema.Schema finalizingSchema, List<Table> tables)
		{
			if (RelatedObjectModel == null)
			{
				RelatedObjectModel = finalizingSchema.EntityModels.FirstOrDefault(
					entityModel => entityModel.Fields.Contains(RelatedPrimaryKey)
					);
				RelatedObjectProjection = RelatedObjectModel;
			}

			if (JunctionTable == null)
			{
				var entityModel = finalizingSchema.EntityModels.First(
					q => q.Fields.Contains(this)
					);

				LocalJunctionColumn = new Column("LocalKey", LocalColumn.SqlDataType);
				RelatedJunctionColumn = new Column("RemoteKey", RelatedPrimaryKey.Column.SqlDataType);
				JunctionTable = new Table($"{entityModel.EntityTable.TableName}_{FieldName}To{RelatedObjectModel.EntityTable.TableName}", new[]
					{
						LocalJunctionColumn, RelatedJunctionColumn
					});
				tables.Add(JunctionTable);
			}

			if (Mapping == null)
			{
				var mappingBuilder = new MappingBuilder(RelatedObjectModel, ElementModel);
				mappingBuilder.AddConvention(CreateInstanceAsNeeded.Instance);
				mappingBuilder.AddConvention(CreateEmbeddedInstanceUsingNotNullColumn.Instance);
				mappingBuilder.AddConvention(CreateSingleRelatedInstanceWhenPresent.Instance);
				mappingBuilder.AddConvention(CopyValueFields.Instance);

				Mapping = mappingBuilder.BuildMapping();
			}
		}

		public MultipleObjectMapper CreateObjectMapper(string identityFieldName)
		{
			return new MultipleObjectMapper<T, TElement, TIdentifier>(identityFieldName, this);
		}
	}
}
