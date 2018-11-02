namespace Silk.Data.SQL.ORM.Schema
{
	public interface IEntityFieldBuilder
	{
		IEntityFieldAssemblage CreateAssemblage(string[] modelPath);
		ProjectionField BuildProjectionField(string sourceName, string fieldName,
			string aliasName, string[] modelPath, EntityFieldJoin join);
		IEntityField Build();
	}

	public class SqlPrimitiveEntityFieldBuilder<TValue, TEntity> : IEntityFieldBuilder
		where TEntity : class
	{
		private readonly EntityFieldDefinition<TValue, TEntity> _entityFieldDefinition;
		private SqlPrimitiveEntityFieldAssemblage<TValue, TEntity> _assemblage;

		public SqlPrimitiveEntityFieldBuilder(EntityFieldDefinition<TValue, TEntity> entityFieldDefinition)
		{
			_entityFieldDefinition = entityFieldDefinition;
		}

		public IEntityFieldAssemblage CreateAssemblage(string[] modelPath)
		{
			_assemblage = new SqlPrimitiveEntityFieldAssemblage<TValue, TEntity>(
				modelPath, this, _entityFieldDefinition
				);
			return _assemblage;
		}

		public ProjectionField BuildProjectionField(string sourceName, string fieldName,
			string aliasName, string[] modelPath, EntityFieldJoin join)
		{
			return new ProjectionField<TValue>(sourceName, fieldName, aliasName, modelPath, join);
		}

		public IEntityField Build()
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
		private readonly EntityFieldDefinition<TValue, TEntity> _entityFieldDefinition;
		private ObjectEntityFieldAssemblage<TValue, TEntity> _assemblage;

		public ObjectEntityFieldBuilder(EntityFieldDefinition<TValue, TEntity> entityFieldDefinition)
		{
			_entityFieldDefinition = entityFieldDefinition;
		}

		public IEntityFieldAssemblage CreateAssemblage(string[] modelPath)
		{
			_assemblage = new ObjectEntityFieldAssemblage<TValue, TEntity>(
				modelPath, this, _entityFieldDefinition
				);
			return _assemblage;
		}

		public ProjectionField BuildProjectionField(string sourceName, string fieldName,
			string aliasName, string[] modelPath, EntityFieldJoin join)
		{
			return new ProjectionField<TValue>(sourceName, fieldName, aliasName, modelPath, join);
		}

		public IEntityField Build()
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
}
