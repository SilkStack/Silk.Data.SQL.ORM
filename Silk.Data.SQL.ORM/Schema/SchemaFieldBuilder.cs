using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using System;
using System.Collections.Generic;

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

	public class SqlPrimitiveSchemaFieldBuilder<TValue, TEntity> : ISchemaFieldBuilder<TEntity>
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
				_typeModel.GetFieldReference(new PathOnlySourceField(_assemblage.ModelPath))
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

		//public ProjectionField BuildProjectionField()
		//{
		//	var sourceName = _entitySchemaAssemblage.TableName;  //  table name or join alias
		//	var fieldName = _entityFieldDefinition.ColumnName;
		//	var aliasName = string.Join("_", _assemblage.ModelPath);
		//	var modelPath = _assemblage.ModelPath;
		//	var join = default(EntityFieldJoin);

		//	return new ProjectionField<TValue>(sourceName, fieldName, aliasName, modelPath, join);
		//}

		//public IEntityField BuildEntityField()
		//{
		//	if (_entityFieldDefinition.SqlDataType == null ||
		//		!_entityFieldDefinition.ModelField.CanRead ||
		//		_entityFieldDefinition.ModelField.IsEnumerable)
		//		return null;

		//	var primaryKeyGenerator = PrimaryKeyGenerator.NotPrimaryKey;
		//	if (_entityFieldDefinition.IsPrimaryKey)
		//		primaryKeyGenerator = GetPrimaryKeyGenerator(_entityFieldDefinition.SqlDataType);
		//	var entityField = new EntityField<TValue, TEntity>(
		//		new[] { new Column(
		//			_entityFieldDefinition.ColumnName, _entityFieldDefinition.SqlDataType, _entityFieldDefinition.IsNullable
		//			) },
		//		_entityFieldDefinition.ModelField.FieldName,
		//		primaryKeyGenerator,
		//		_assemblage.ModelPath,
		//		_entityFieldDefinition.ModelField);
		//	return entityField;
		//}

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

	public class ObjectEntityFieldBuilder<TValue, TEntity> : ISchemaFieldBuilder<TEntity>
		where TEntity : class
	{
		private readonly IEntitySchemaAssemblage _entitySchemaAssemblage;
		private readonly IReadOnlyCollection<IEntitySchemaAssemblage> _entitySchemaAssemblages;
		private readonly SchemaFieldDefinition<TValue, TEntity> _entityFieldDefinition;
		private ObjectSchemaFieldAssemblage<TValue, TEntity> _assemblage;

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
			throw new System.NotImplementedException();
		}

		public FieldOperations<TEntity> BuildFieldOperations()
		{
			throw new NotImplementedException();
		}

		public ISchemaFieldAssemblage<TEntity> CreateAssemblage(string[] modelPath)
		{
			_assemblage = new ObjectSchemaFieldAssemblage<TValue, TEntity>(
				modelPath, this, _entityFieldDefinition
				);
			return _assemblage;
		}

		//public ProjectionField BuildProjectionField()
		//{
		//	var fieldTypeAssemblage = _entitySchemaAssemblages.FirstOrDefault(
		//			q => q.EntityType == typeof(TValue)
		//			);
		//	if (fieldTypeAssemblage == null)
		//	{
		//		var sourceName = _entitySchemaAssemblage.TableName;
		//		var columnName = string.Join("_", _assemblage.ModelPath);
		//		var aliasName = $"__NULL_CHECK_{string.Join("_", _assemblage.ModelPath)}";
		//		return new EmbeddedPocoNullCheckProjection(
		//			sourceName, columnName, aliasName, _assemblage.ModelPath, null
		//			);
		//	}
		//	else
		//	{
		//		return null;
		//	}
		//}

		//public IEntityField BuildEntityField()
		//{
		//	var fieldTypeAssemblage = _entitySchemaAssemblages.FirstOrDefault(
		//		q => q.EntityType == typeof(TValue)
		//		);
		//	if (fieldTypeAssemblage == null)
		//	{
		//		//  embedded poco null check
		//		return new EmbeddedPocoField<TValue>(_entityFieldDefinition.ModelField, _assemblage.ModelPath);
		//	}
		//	else
		//	{
		//		return null;
		//	}
		//}
	}
}
