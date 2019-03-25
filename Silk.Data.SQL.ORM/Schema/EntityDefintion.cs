using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Defines options for an entity type.
	/// </summary>
	public abstract class EntityDefinition
	{
		public abstract Type EntityType { get; }
		public abstract TypeModel TypeModel { get; }
		public abstract IEnumerable<IndexBuilder> IndexBuilders { get; }

		public string TableName { get; set; }
		public EntityDefinition SetTableName(string tableName)
		{
			TableName = tableName;
			return this;
		}

		public abstract IEnumerable<EntityField> BuildEntityFields(
			IFieldBuilder fieldBuilder,
			EntityDefinition entityDefinition,
			TypeModel typeModel,
			IEnumerable<IField> relativeParentFields = null,
			IEnumerable<IField> fullParentFields = null,
			IQueryReference source = null
			);
		public abstract EntityModel BuildModel(
			ClassToEntityIntersectionAnalyzer classToEntityAnalyzer,
			IIntersectionAnalyzer<EntityModel, EntityField, TypeModel, PropertyInfoField> modelToTypeAnalyzer, 
			IEnumerable<EntityField> entityFields, IEnumerable<Index> indexes);
	}

	/// <summary>
	/// Defines options for an entity type.
	/// </summary>
	public class EntityDefinition<T> : EntityDefinition
		where T : class
	{
		private readonly Dictionary<string, IndexBuilder<T>> _indexBuilders = new Dictionary<string, IndexBuilder<T>>();

		public override TypeModel TypeModel { get; } = TypeModel.GetModelOf<T>();

		public override Type EntityType { get; } = typeof(T);

		public override IEnumerable<IndexBuilder> IndexBuilders => _indexBuilders.Values;

		public EntityDefinition()
		{
			TableName = typeof(T).Name;
		}

		public EntityDefinition<T> Index(string indexName, params Expression<Func<T, object>>[] indexFields)
			=> Index(indexName, false, indexFields);

		public EntityDefinition<T> Index(string indexName, bool uniqueConstraint, params Expression<Func<T, object>>[] indexFields)
		{
			if (!_indexBuilders.TryGetValue(indexName, out var indexBuilder))
			{
				indexBuilder = new IndexBuilder<T>(indexName);
				_indexBuilders.Add(indexName, indexBuilder);
			}

			indexBuilder.HasUniqueConstraint = uniqueConstraint;
			indexBuilder.AddFields(indexFields);
			return this;
		}

		public new EntityDefinition<T> SetTableName(string tableName)
		{
			TableName = tableName;
			return this;
		}

		public override EntityModel BuildModel(
			ClassToEntityIntersectionAnalyzer classToEntityAnalyzer,
			IIntersectionAnalyzer<EntityModel, EntityField, TypeModel, PropertyInfoField> modelToTypeAnalyzer,
			IEnumerable<EntityField> entityFields, IEnumerable<Index> indexes)
		{
			return new EntityModel<T>(
				classToEntityAnalyzer, modelToTypeAnalyzer,
				entityFields.OfType<EntityField<T>>(), TableName, indexes
				);
		}

		public override IEnumerable<EntityField> BuildEntityFields(IFieldBuilder fieldBuilder,
			EntityDefinition entityDefinition,
			TypeModel typeModel, IEnumerable<IField> relativeParentFields = null,
			IEnumerable<IField> fullParentFields = null, IQueryReference source = null)
			=> fieldBuilder.BuildEntityFields<T>(this, entityDefinition, typeModel, relativeParentFields,
				fullParentFields, source);
		
	}
}
