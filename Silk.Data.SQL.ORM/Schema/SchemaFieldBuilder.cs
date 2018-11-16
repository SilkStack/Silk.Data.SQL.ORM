using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public interface ISchemaFieldBuilder
	{
	}

	public interface ISchemaFieldBuilder<TEntity> : ISchemaFieldBuilder
		where TEntity : class
	{
		ISchemaFieldAssemblage<TEntity> CreateAssemblage(string[] modelPath);
		ISchemaField<TEntity> Build();
		FieldOperations<TEntity> BuildFieldOperations();
	}

	public class SchemaFieldBuilderBase<TValue, TEntity>
	{
		protected IFieldReference GetFieldReference(string[] path)
		{
			var sourceField = new PathOnlySourceField(path);
			return TypeModel.GetModelOf<TEntity>().GetFieldReference(sourceField);
		}

		private class PathOnlySourceField : ISourceField
		{
			public IModel RootModel => throw new NotImplementedException();

			public string[] FieldPath { get; }

			public ISourceField[] Fields => throw new NotImplementedException();

			public string FieldName => throw new NotImplementedException();

			public Type FieldType => throw new NotImplementedException();

			public bool CanRead => throw new NotImplementedException();

			public bool CanWrite => throw new NotImplementedException();

			public bool IsEnumerable => throw new NotImplementedException();

			public Type ElementType => throw new NotImplementedException();

			public TypeModel FieldTypeModel => throw new NotImplementedException();

			public PathOnlySourceField(string[] fieldPath)
			{
				FieldPath = fieldPath;
			}

			public MappingBinding CreateBinding<TTo>(IMappingBindingFactory bindingFactory, ITargetField toField)
			{
				throw new NotImplementedException();
			}

			public MappingBinding CreateBinding<TTo, TBindingOption>(IMappingBindingFactory<TBindingOption> bindingFactory, ITargetField toField, TBindingOption bindingOption)
			{
				throw new NotImplementedException();
			}

			public void Transform(IModelTransformer transformer)
			{
				throw new NotImplementedException();
			}
		}
	}

	public class SqlPrimitiveSchemaFieldBuilder<TValue, TEntity> : SchemaFieldBuilderBase<TValue, TEntity>, ISchemaFieldBuilder<TEntity>
		where TEntity : class
	{
		private readonly TypeModel<TEntity> _typeModel = TypeModel.GetModelOf<TEntity>();

		private readonly IEntitySchemaAssemblage _entitySchemaAssemblage;
		private readonly SchemaFieldDefinition<TValue, TEntity> _entityFieldDefinition;
		private SqlPrimitiveSchemaFieldAssemblage<TValue, TEntity> _assemblage;
		private SqlPrimitiveSchemaField<TValue, TEntity> _builtField;

		public SqlPrimitiveSchemaFieldBuilder(
			IEntitySchemaAssemblage entitySchemaAssemblage,
			SchemaFieldDefinition<TValue, TEntity> entityFieldDefinition
			)
		{
			_entitySchemaAssemblage = entitySchemaAssemblage;
			_entityFieldDefinition = entityFieldDefinition;
		}

		public ISchemaFieldAssemblage<TEntity> CreateAssemblage(string[] modelPath)
		{
			var primaryKeyGenerator = PrimaryKeyGenerator.NotPrimaryKey;
			if (_entityFieldDefinition.IsPrimaryKey)
				primaryKeyGenerator = GetPrimaryKeyGenerator(_entityFieldDefinition.SqlDataType);

			_assemblage = new SqlPrimitiveSchemaFieldAssemblage<TValue, TEntity>(
				modelPath, this, _entityFieldDefinition, primaryKeyGenerator
				);
			return _assemblage;
		}

		public ISchemaField<TEntity> Build()
		{
			_builtField = new SqlPrimitiveSchemaField<TValue, TEntity>(
				_entityFieldDefinition.ModelField.FieldName, _assemblage.Column, _assemblage.PrimaryKeyGenerator,
				GetFieldReference(_assemblage.ModelPath)
				);
			return _builtField;
		}

		public FieldOperations<TEntity> BuildFieldOperations()
		{
			if (_builtField == null)
				throw new InvalidOperationException("Field not built, call Build() before BuildFieldOperations().");
			return new FieldOperations<TEntity>(
				new SqlPrimitiveFieldExpressionFactory<TValue, TEntity>(_builtField)
				);
		}

		private static PrimaryKeyGenerator GetPrimaryKeyGenerator(SqlDataType sqlDataType)
		{
			switch (sqlDataType.BaseType)
			{
				case SqlBaseType.TinyInt:
				case SqlBaseType.SmallInt:
				case SqlBaseType.Int:
				case SqlBaseType.BigInt:
					return PrimaryKeyGenerator.ServerGenerated;
				default:
					return PrimaryKeyGenerator.ClientGenerated;
			}
		}
	}

	public class ObjectEntityFieldBuilder<TValue, TEntity> : SchemaFieldBuilderBase<TValue, TEntity>, ISchemaFieldBuilder<TEntity>
		where TEntity : class
	{
		private readonly IEntitySchemaAssemblage _entitySchemaAssemblage;
		private readonly IReadOnlyCollection<IEntitySchemaAssemblage> _entitySchemaAssemblages;
		private readonly SchemaFieldDefinition<TValue, TEntity> _entityFieldDefinition;
		private ObjectSchemaFieldAssemblage<TValue, TEntity> _assemblage;
		private FieldOperations<TEntity> _fieldOperations;

		public ObjectEntityFieldBuilder(
			IEntitySchemaAssemblage entitySchemaAssemblage,
			IReadOnlyCollection<IEntitySchemaAssemblage> entitySchemaAssemblages,
			SchemaFieldDefinition<TValue, TEntity> entityFieldDefinition
			)
		{
			_entitySchemaAssemblage = entitySchemaAssemblage;
			_entitySchemaAssemblages = entitySchemaAssemblages;
			_entityFieldDefinition = entityFieldDefinition;
		}

		public ISchemaField<TEntity> Build()
		{
			var isEmbeddedObject = !_entitySchemaAssemblages.Any(q => q.EntityType == typeof(TValue));
			if (isEmbeddedObject)
				return BuildAsEmbeddedObject();
			throw new NotImplementedException();
		}

		private ISchemaField<TEntity> BuildAsEmbeddedObject()
		{
			var field = new EmbeddedObjectNullCheckSchemaField<TValue, TEntity>(
				_entityFieldDefinition.ModelField.FieldName,
				_entityFieldDefinition.ColumnName ?? string.Join("_", _assemblage.ModelPath),
				GetFieldReference(_assemblage.ModelPath)
				);
			_fieldOperations = new FieldOperations<TEntity>(
				new EmbeddedObjectNullCheckExpressionFactory<TValue, TEntity>(field)
				);
			return field;
		}

		public FieldOperations<TEntity> BuildFieldOperations()
		{
			if (_fieldOperations == null)
				throw new InvalidOperationException("Field hasn't been built yet, call Build() before calling BuildFieldOperations().");
			return _fieldOperations;
		}

		public ISchemaFieldAssemblage<TEntity> CreateAssemblage(string[] modelPath)
		{
			_assemblage = new ObjectSchemaFieldAssemblage<TValue, TEntity>(
				modelPath, this, _entityFieldDefinition
				);
			return _assemblage;
		}
	}
}
