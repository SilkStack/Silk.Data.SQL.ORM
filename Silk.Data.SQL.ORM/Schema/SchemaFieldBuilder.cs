using Silk.Data.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public interface ISchemaFieldBuilder
	{
		EntityJoinBuilder CreateJoin(int joinCount);
	}

	public interface ISchemaFieldBuilder<TEntity> : ISchemaFieldBuilder
		where TEntity : class
	{
		ISchemaFieldAssemblage<TEntity> CreateAssemblage(string[] modelPath, EntityJoinBuilder join);
		IEnumerable<(ISchemaField<TEntity> Field, FieldOperations<TEntity> Operations)> Build(int currentFieldCount);
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

		public SqlPrimitiveSchemaFieldBuilder(
			IEntitySchemaAssemblage<TEntity> entitySchemaAssemblage,
			SchemaFieldDefinition<TValue, TEntity> entityFieldDefinition
			)
		{
			_entitySchemaAssemblage = entitySchemaAssemblage;
			_entityFieldDefinition = entityFieldDefinition;
		}

		public ISchemaFieldAssemblage<TEntity> CreateAssemblage(string[] modelPath, EntityJoinBuilder join)
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

		public IEnumerable<(ISchemaField<TEntity> Field, FieldOperations<TEntity> Operations)> Build(int currentFieldCount)
		{
			var field = new SqlPrimitiveSchemaField<TValue, TEntity>(
				_entityFieldDefinition.ModelField.FieldName, _assemblage.Column,
				_assemblage.PrimaryKeyGenerator, GetFieldReference(_assemblage.ModelPath),
				_assemblage.Join?.Build(), _assemblage.ModelPath, $"_valueField_{currentFieldCount}"
				);
			var operations = new FieldOperations<TEntity>(
				new SqlPrimitiveFieldExpressionFactory<TValue, TEntity>(field)
				);
			return new (ISchemaField<TEntity>, FieldOperations<TEntity>)[]
			{
				(field, operations)
			};
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

		public EntityJoinBuilder CreateJoin(int joinCount)
		{
			throw new NotImplementedException();
		}
	}

	public class ObjectEntityFieldBuilder<TValue, TEntity> : SchemaFieldBuilderBase<TValue, TEntity>, ISchemaFieldBuilder<TEntity>
		where TEntity : class
		where TValue : class
	{
		private bool IsEmbeddedObject => !_entitySchemaAssemblages.Any(q => q.EntityType == typeof(TValue));

		private readonly IEntitySchemaAssemblage<TEntity> _entitySchemaAssemblage;
		private readonly IReadOnlyCollection<IEntitySchemaAssemblage> _entitySchemaAssemblages;
		private readonly SchemaFieldDefinition<TValue, TEntity> _entityFieldDefinition;
		private ObjectSchemaFieldAssemblage<TValue, TEntity> _assemblage;

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

		public IEnumerable<(ISchemaField<TEntity> Field, FieldOperations<TEntity> Operations)> Build(int currentFieldCount)
		{
			if (IsEmbeddedObject)
				return BuildAsEmbeddedObject(currentFieldCount);
			return BuildAsJoinedObject(currentFieldCount);
		}

		private IEnumerable<(ISchemaField<TEntity> Field, FieldOperations<TEntity> Operations)> BuildAsJoinedObject(int currentFieldCount)
		{
			var joinedSchemaAssemblage = _entitySchemaAssemblages.OfType<IEntitySchemaAssemblage<TValue>>().First();
			var primaryKeyFieldAssemblages = joinedSchemaAssemblage.Fields.Where(q => q.PrimaryKeyGenerator != PrimaryKeyGenerator.NotPrimaryKey).ToArray();

			if (primaryKeyFieldAssemblages.Length == 0)
				throw new InvalidOperationException("Related objects need to have at least 1 primary key.");

			foreach (var primaryKeyFieldAssemblage in primaryKeyFieldAssemblages)
			{
				var columnName = _entityFieldDefinition.ColumnName ??
						string.Join("_", _assemblage.ModelPath.Concat(primaryKeyFieldAssemblage.ModelPath));

				yield return primaryKeyFieldAssemblage.CreateJoinedSchemaFieldAndOperationsPair<TEntity>(
					_entityFieldDefinition.ModelField.FieldName,
					columnName,
					GetFieldReference(_assemblage.ModelPath),
					_assemblage.ModelPath,
					_assemblage.Join?.Build(),
					_entitySchemaAssemblage,
					$"_fkField_{currentFieldCount}"
					);
			}
		}

		private IEnumerable<(ISchemaField<TEntity> Field, FieldOperations<TEntity> Operations)> BuildAsEmbeddedObject(int currentFieldCount)
		{
			var columnName = _entityFieldDefinition.ColumnName ?? string.Join("_", _assemblage.ModelPath);

			var field = new EmbeddedObjectNullCheckSchemaField<TValue, TEntity>(
				_entityFieldDefinition.ModelField.FieldName,
				columnName,
				GetFieldReference(_assemblage.ModelPath),
				_assemblage.Join?.Build(),
				_entitySchemaAssemblage,
				_assemblage.ModelPath,
				$"_embeddedField_{currentFieldCount}"
				);
			var operations = new FieldOperations<TEntity>(
				new EmbeddedObjectNullCheckExpressionFactory<TValue, TEntity>(field)
				);

			return new (ISchemaField<TEntity> Field, FieldOperations<TEntity> Operations)[]
			{
				(field, operations)
			};
		}

		public ISchemaFieldAssemblage<TEntity> CreateAssemblage(string[] modelPath, EntityJoinBuilder join)
		{
			_assemblage = new ObjectSchemaFieldAssemblage<TValue, TEntity>(
				modelPath, this, _entityFieldDefinition, join
				);
			return _assemblage;
		}

		public EntityJoinBuilder CreateJoin(int joinCount)
		{
			if (IsEmbeddedObject)
				throw new InvalidOperationException("Can only create joins for joined entities.");

			var joinedSchemaAssemblage = _entitySchemaAssemblages.OfType<IEntitySchemaAssemblage<TValue>>().First();
			var primaryKeyFieldAssemblages = joinedSchemaAssemblage.Fields.Where(q => q.PrimaryKeyGenerator != PrimaryKeyGenerator.NotPrimaryKey).ToArray();

			if (primaryKeyFieldAssemblages.Length == 0)
				throw new InvalidOperationException("Related objects need to have at least 1 primary key.");

			var localColumnNames = new List<string>();
			var foreignColumnNames = new List<string>();
			foreach (var primaryKeyFieldAssemblage in primaryKeyFieldAssemblages)
			{
				var localColumnName = _entityFieldDefinition.ColumnName ??
						string.Join("_", _assemblage.ModelPath.Concat(primaryKeyFieldAssemblage.ModelPath));

				localColumnNames.Add(localColumnName);
				foreignColumnNames.Add(primaryKeyFieldAssemblage.Column.ColumnName);
			}

			EntityJoinBuilder[] dependencyJoins;
			if (_assemblage.Join == null)
				dependencyJoins = new EntityJoinBuilder[0];
			else
				dependencyJoins = new[] { _assemblage.Join };

			return new EntityJoinBuilder(
				joinedSchemaAssemblage.TableName,
				$"__join_table_{joinCount}",
				_assemblage.Join?.TableName ?? _entitySchemaAssemblage.TableName,
				localColumnNames,
				foreignColumnNames,
				dependencyJoins
				);
		}
	}
}
