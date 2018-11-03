using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public interface IEntityFieldBuilder
	{
		IEntityFieldAssemblage CreateAssemblage(string[] modelPath);
		ProjectionField BuildProjectionField();
		IEntityField BuildEntityField();
	}

	public class SqlPrimitiveEntityFieldBuilder<TValue, TEntity> : IEntityFieldBuilder
		where TEntity : class
	{
		private readonly IEntitySchemaAssemblage _entitySchemaAssemblage;
		private readonly EntityFieldDefinition<TValue, TEntity> _entityFieldDefinition;
		private SqlPrimitiveEntityFieldAssemblage<TValue, TEntity> _assemblage;

		public SqlPrimitiveEntityFieldBuilder(
			IEntitySchemaAssemblage entitySchemaAssemblage,
			EntityFieldDefinition<TValue, TEntity> entityFieldDefinition
			)
		{
			_entitySchemaAssemblage = entitySchemaAssemblage;
			_entityFieldDefinition = entityFieldDefinition;
		}

		public IEntityFieldAssemblage CreateAssemblage(string[] modelPath)
		{
			_assemblage = new SqlPrimitiveEntityFieldAssemblage<TValue, TEntity>(
				modelPath, this, _entityFieldDefinition
				);
			return _assemblage;
		}

		public ProjectionField BuildProjectionField()
		{
			var sourceName = _entitySchemaAssemblage.TableName;  //  table name or join alias
			var fieldName = _entityFieldDefinition.ColumnName;
			var aliasName = string.Join("_", _assemblage.ModelPath);
			var modelPath = _assemblage.ModelPath;
			var join = default(EntityFieldJoin);

			return new ProjectionField<TValue>(sourceName, fieldName, aliasName, modelPath, join);
		}

		public IEntityField BuildEntityField()
		{
			if (_entityFieldDefinition.SqlDataType == null ||
				!_entityFieldDefinition.ModelField.CanRead ||
				_entityFieldDefinition.ModelField.IsEnumerable)
				return null;

			var primaryKeyGenerator = PrimaryKeyGenerator.NotPrimaryKey;
			if (_entityFieldDefinition.IsPrimaryKey)
				primaryKeyGenerator = GetPrimaryKeyGenerator(_entityFieldDefinition.SqlDataType);
			var entityField = new EntityField<TValue, TEntity>(
				new[] { new Column(
					_entityFieldDefinition.ColumnName, _entityFieldDefinition.SqlDataType, _entityFieldDefinition.IsNullable
					) },
				_entityFieldDefinition.ModelField.FieldName,
				primaryKeyGenerator,
				_assemblage.ModelPath,
				_entityFieldDefinition.ModelField);
			return entityField;
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

	public class ObjectEntityFieldBuilder<TValue, TEntity> : IEntityFieldBuilder
		where TEntity : class
	{
		private readonly IEntitySchemaAssemblage _entitySchemaAssemblage;
		private readonly IReadOnlyCollection<IEntitySchemaAssemblage> _entitySchemaAssemblages;
		private readonly EntityFieldDefinition<TValue, TEntity> _entityFieldDefinition;
		private ObjectEntityFieldAssemblage<TValue, TEntity> _assemblage;

		public ObjectEntityFieldBuilder(
			IEntitySchemaAssemblage entitySchemaAssemblage,
			IReadOnlyCollection<IEntitySchemaAssemblage> entitySchemaAssemblages,
			EntityFieldDefinition<TValue, TEntity> entityFieldDefinition
			)
		{
			_entitySchemaAssemblage = entitySchemaAssemblage;
			_entitySchemaAssemblages = entitySchemaAssemblages;
			_entityFieldDefinition = entityFieldDefinition;
		}

		public IEntityFieldAssemblage CreateAssemblage(string[] modelPath)
		{
			_assemblage = new ObjectEntityFieldAssemblage<TValue, TEntity>(
				modelPath, this, _entityFieldDefinition
				);
			return _assemblage;
		}

		public ProjectionField BuildProjectionField()
		{
			var fieldTypeAssemblage = _entitySchemaAssemblages.FirstOrDefault(
					q => q.EntityType == typeof(TValue)
					);
			if (fieldTypeAssemblage == null)
			{
				var sourceName = _entitySchemaAssemblage.TableName;
				var columnName = string.Join("_", _assemblage.ModelPath);
				var aliasName = $"__NULL_CHECK_{string.Join("_", _assemblage.ModelPath)}";
				return new EmbeddedPocoNullCheckProjection(
					sourceName, columnName, aliasName, _assemblage.ModelPath, null
					);
			}
			else
			{
				return null;
			}
		}

		public IEntityField BuildEntityField()
		{
			var fieldTypeAssemblage = _entitySchemaAssemblages.FirstOrDefault(
				q => q.EntityType == typeof(TValue)
				);
			if (fieldTypeAssemblage == null)
			{
				//  embedded poco null check
				return new EmbeddedPocoField<TValue>(_entityFieldDefinition.ModelField, _assemblage.ModelPath);
			}
			else
			{
				return null;
			}
		}
	}
}
