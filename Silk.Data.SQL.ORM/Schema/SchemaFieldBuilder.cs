using Silk.Data.Modelling;
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
		ISchemaFieldAssemblage<TEntity> CreateAssemblage(string[] modelPath, EntityFieldJoin join);
		ISchemaField<TEntity> Build(IEnumerable<ISchemaField<TEntity>> parentFields);
		FieldOperations<TEntity> BuildFieldOperations();
	}

	public class SchemaFieldBuilderBase<TValue, TEntity>
	{
		protected IFieldReference GetFieldReference(string[] path)
		{
			var sourceField = new PathOnlySourceField(path);
			return TypeModel.GetModelOf<TEntity>().GetFieldReference(sourceField);
		}
	}

	public class SqlPrimitiveSchemaFieldBuilder<TValue, TEntity> : SchemaFieldBuilderBase<TValue, TEntity>, ISchemaFieldBuilder<TEntity>
		where TEntity : class
	{
		private readonly TypeModel<TEntity> _typeModel = TypeModel.GetModelOf<TEntity>();

		private readonly IEntitySchemaAssemblage<TEntity> _entitySchemaAssemblage;
		private readonly SchemaFieldDefinition<TValue, TEntity> _entityFieldDefinition;
		private SqlPrimitiveSchemaFieldAssemblage<TValue, TEntity> _assemblage;
		private SqlPrimitiveSchemaField<TValue, TEntity> _builtField;

		public SqlPrimitiveSchemaFieldBuilder(
			IEntitySchemaAssemblage<TEntity> entitySchemaAssemblage,
			SchemaFieldDefinition<TValue, TEntity> entityFieldDefinition
			)
		{
			_entitySchemaAssemblage = entitySchemaAssemblage;
			_entityFieldDefinition = entityFieldDefinition;
		}

		public ISchemaFieldAssemblage<TEntity> CreateAssemblage(string[] modelPath, EntityFieldJoin join)
		{
			var primaryKeyGenerator = PrimaryKeyGenerator.NotPrimaryKey;
			if (_entityFieldDefinition.IsPrimaryKey)
				primaryKeyGenerator = GetPrimaryKeyGenerator(_entityFieldDefinition.SqlDataType);

			_assemblage = new SqlPrimitiveSchemaFieldAssemblage<TValue, TEntity>(
				modelPath, this, _entityFieldDefinition, primaryKeyGenerator,
				_entitySchemaAssemblage, join
				);
			return _assemblage;
		}

		public ISchemaField<TEntity> Build(IEnumerable<ISchemaField<TEntity>> parentFields)
		{
			_builtField = new SqlPrimitiveSchemaField<TValue, TEntity>(
				_entityFieldDefinition.ModelField.FieldName, _assemblage.Column,
				_assemblage.PrimaryKeyGenerator, GetFieldReference(_assemblage.ModelPath),
				_assemblage.Join
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
		where TValue : class
	{
		private readonly IEntitySchemaAssemblage<TEntity> _entitySchemaAssemblage;
		private readonly IReadOnlyCollection<IEntitySchemaAssemblage> _entitySchemaAssemblages;
		private readonly SchemaFieldDefinition<TValue, TEntity> _entityFieldDefinition;
		private ObjectSchemaFieldAssemblage<TValue, TEntity> _assemblage;
		private FieldOperations<TEntity> _fieldOperations;

		public ObjectEntityFieldBuilder(
			IEntitySchemaAssemblage<TEntity> entitySchemaAssemblage,
			IReadOnlyCollection<IEntitySchemaAssemblage> entitySchemaAssemblages,
			SchemaFieldDefinition<TValue, TEntity> entityFieldDefinition
			)
		{
			_entitySchemaAssemblage = entitySchemaAssemblage;
			_entitySchemaAssemblages = entitySchemaAssemblages;
			_entityFieldDefinition = entityFieldDefinition;
		}

		public ISchemaField<TEntity> Build(IEnumerable<ISchemaField<TEntity>> parentFields)
		{
			var isEmbeddedObject = !_entitySchemaAssemblages.Any(q => q.EntityType == typeof(TValue));
			if (isEmbeddedObject)
				return BuildAsEmbeddedObject(parentFields);
			return BuildAsJoinedObject(parentFields);
		}

		private ISchemaField<TEntity> BuildAsJoinedObject(IEnumerable<ISchemaField<TEntity>> parentFields)
		{
			var joinedSchemaAssemblage = _entitySchemaAssemblages.OfType<IEntitySchemaAssemblage<TValue>>().First();
			var primaryKeyFieldAssemblages = joinedSchemaAssemblage.Fields.Where(q => q.PrimaryKeyGenerator != PrimaryKeyGenerator.NotPrimaryKey);
			foreach (var primaryKeyFieldAssemblage in primaryKeyFieldAssemblages)
			{
				var columnName = _entityFieldDefinition.ColumnName ??
						string.Join("_", _assemblage.ModelPath.Concat(primaryKeyFieldAssemblage.ModelPath));

				var (field, fieldOperations) = primaryKeyFieldAssemblage.CreateJoinedSchemaFieldAndOperationsPair<TEntity>(
					_entityFieldDefinition.ModelField.FieldName,
					columnName,
					GetFieldReference(_assemblage.ModelPath),
					_assemblage.ModelPath,
					_assemblage.Join,
					_entitySchemaAssemblage
					);
				_fieldOperations = fieldOperations;
				return field;
			}
			throw new InvalidOperationException("Related objects need to have at least 1 primary key.");
		}

		private ISchemaField<TEntity> BuildAsEmbeddedObject(IEnumerable<ISchemaField<TEntity>> parentFields)
		{
			var columnName = _entityFieldDefinition.ColumnName ?? string.Join("_", _assemblage.ModelPath);

			var field = new EmbeddedObjectNullCheckSchemaField<TValue, TEntity>(
				_entityFieldDefinition.ModelField.FieldName,
				columnName,
				GetFieldReference(_assemblage.ModelPath),
				_assemblage.Join,
				_entitySchemaAssemblage
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

		public ISchemaFieldAssemblage<TEntity> CreateAssemblage(string[] modelPath, EntityFieldJoin join)
		{
			_assemblage = new ObjectSchemaFieldAssemblage<TValue, TEntity>(
				modelPath, this, _entityFieldDefinition, join
				);
			return _assemblage;
		}
	}
}
