﻿using System;
using System.Collections.Generic;
using System.Linq;
using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Modelling
{
	public abstract class EntityModelTransformer
	{
		public abstract IEntityField[] Fields { get; }
	}

	public class EntityModelTransformer<T> : EntityModelTransformer, IModelTransformer
	{
		private static readonly Type[] _autoIncrementTypes = new[]
		{
			typeof(ushort), typeof(short),
			typeof(uint), typeof(int),
			typeof(ulong), typeof(long)
		};

		public bool FieldsAdded { get; set; }

		public override IEntityField[] Fields => _entityFields.Values.ToArray();

		private string _columnNamePrefix = "";
		private string _entityTableName;
		private readonly List<Column> _entityColumns =
			new List<Column>();
		private Dictionary<string, IEntityField> _entityFields =
			new Dictionary<string, IEntityField>();
		private readonly EntitySchemaOptions<T> _schemaOptions;
		private readonly SchemaBuilder _schemaBuilder;
		private readonly Stack<string> _pathStack = new Stack<string>();

		public EntityModelTransformer(EntitySchemaOptions<T> schemaOptions, SchemaBuilder schemaBuilder)
		{
			_schemaOptions = schemaOptions;
			_schemaBuilder = schemaBuilder;
		}

		private void ModelPrimitiveField<TData>(IField<TData> field, EntityFieldOptions options, string sqlColumnName)
		{
			var isIdField = field.FieldName.ToLowerInvariant() == "id" && string.IsNullOrEmpty(_columnNamePrefix);
			var isPrimaryKey = isIdField;
			var isAutoIncrement = false;
			var isAutoGenerated = false;
			if (options?.IsPrimaryKey == true)
				isPrimaryKey = true;

			if (isPrimaryKey)
			{
				if (options?.IsAutoGenerate == true || isIdField)
				{
					if (field.FieldType == typeof(Guid))
					{
						isAutoGenerated = true;
					}
					else if (_autoIncrementTypes.Contains(field.FieldType) || isIdField)
					{
						isAutoIncrement = true;
					}
				}
			}

			var column = SqlDataTypes.CreateColumn(field, options, sqlColumnName, isPrimaryKey, isAutoIncrement, isAutoGenerated);

			_entityColumns.Add(column);
			_entityFields.Add(field.FieldName,
				new ValueField<TData>(field.FieldName, field.CanRead, field.CanWrite, false, null, column)
				);

			FieldsAdded = true;
		}

		private void ModelSingleObjectRelationship<TData>(IField<TData> field, EntityFieldOptions options, string sqlColumnName,
			EntityModelTransformer relatedTypeTransformer)
		{
			var primaryKeyField = relatedTypeTransformer.Fields.OfType<IValueField>()
				.FirstOrDefault(q => q.Column.IsPrimaryKey);
			if (primaryKeyField == null)
			{
				//  primary key hasn't been declared yet (if it ever will), we can't model the relationship yet
				return;
			}

			var primaryKeyFieldOptions = _schemaOptions.GetFieldOptions(_pathStack.Reverse().Concat(new[] { primaryKeyField.FieldName }).ToArray());
			var localColumnName = primaryKeyFieldOptions?.ConfiguredColumnName ?? sqlColumnName;
			var isPrimaryKey = primaryKeyFieldOptions?.IsPrimaryKey ?? false;
			var localColumn = new Column(localColumnName, primaryKeyField.Column.SqlDataType, isPrimaryKey: true);
			_entityColumns.Add(localColumn);
			_entityFields.Add(field.FieldName,
				new SingleRelatedObjectField<TData>(field.FieldName, field.CanRead, field.CanWrite, false, null, null, primaryKeyField, localColumn, null)
				);

			FieldsAdded = true;
		}

		private void ModelEmbeddedObject<TData>(IField<TData> field, EntityFieldOptions options, string columnPrefix)
		{
			var existingColumnPrefix = _columnNamePrefix;
			var existingEntityFields = _entityFields;

			_columnNamePrefix = columnPrefix;
			_entityFields = new Dictionary<string, IEntityField>();

			foreach (var subField in field.FieldTypeModel.Fields)
			{
				subField.Transform(this);
			}
			var subObjectFields = _entityFields;

			_entityFields = existingEntityFields;
			_columnNamePrefix = existingColumnPrefix;

			var nullCheckColumn = new Column(columnPrefix.TrimEnd('_'), SqlDataType.Bit());
			_entityColumns.Add(nullCheckColumn);
			_entityFields.Add(field.FieldName,
				new EmbeddedObjectField<TData>(field.FieldName, field.CanRead, field.CanWrite, false, null, subObjectFields.Values, nullCheckColumn)
				);
		}

		private void RevisitEmbeddedObject<TData>(IField<TData> field, EntityFieldOptions options, string columnPrefix, IEmbeddedObjectField embeddedObjectField)
		{
			var existingColumnPrefix = _columnNamePrefix;
			var existingEntityFields = _entityFields;

			_columnNamePrefix = columnPrefix;
			_entityFields = embeddedObjectField.EmbeddedFields.ToDictionary(q => q.FieldName);

			foreach (var subField in field.FieldTypeModel.Fields)
			{
				subField.Transform(this);
			}

			var subObjectFields = _entityFields;

			_entityFields = existingEntityFields;
			_columnNamePrefix = existingColumnPrefix;

			_entityFields.Remove(field.FieldName);
			_entityFields.Add(field.FieldName,
				new EmbeddedObjectField<TData>(field.FieldName, field.CanRead, field.CanWrite, false, null, subObjectFields.Values, embeddedObjectField.NullCheckColumn)
				);
		}

		private void ModelManyObjectRelationship<TData>(IField<TData> field, EntityFieldOptions options, string sqlColumnName,
			EntityModelTransformer relatedTypeTransformer)
		{
			var localPrimaryKey = _entityFields.Select(q => q.Value).OfType<IValueField>().FirstOrDefault(q => q.Column.IsPrimaryKey);
			if (localPrimaryKey == null)
				return;

			var objectPrimaryKey = relatedTypeTransformer.Fields.OfType<IValueField>()
						.FirstOrDefault(q => q.Column.IsPrimaryKey);
			if (objectPrimaryKey == null)
				return;

			var objectFieldType = typeof(ManyRelatedObjectField<,,>)
				.MakeGenericType(typeof(TData), field.ElementType, localPrimaryKey.FieldType);

			_entityFields.Add(field.FieldName,
				Activator.CreateInstance(objectFieldType, field.FieldName, field.CanRead, field.CanWrite, true, field.ElementType, localPrimaryKey.Column, null, null, null,
					null, objectPrimaryKey, localPrimaryKey, null) as IEntityField
				);
		}

		public void VisitField<TData>(IField<TData> field)
		{
			if (!field.CanRead)
				return;

			_pathStack.Push(field.FieldName);
			var options = _schemaOptions.GetFieldOptions(_pathStack.Reverse().ToArray());
			var sqlColumnName = options?.ConfiguredColumnName;
			if (string.IsNullOrWhiteSpace(sqlColumnName))
				sqlColumnName = $"{_columnNamePrefix}{field.FieldName}";

			try
			{
				if (_entityFields.TryGetValue(field.FieldName, out var existingField))
				{
					if (existingField is IEmbeddedObjectField embeddedObjectField)
					{
						RevisitEmbeddedObject<TData>(field, options, $"{sqlColumnName}_", embeddedObjectField);
					}
					return;
				}

				if (!field.IsEnumerable)
				{
					if (SqlDataTypes.IsSQLPrimitiveType(field.FieldType))
					{
						ModelPrimitiveField(field, options, sqlColumnName);
						return;
					}
					var relatedTypeTransformer = _schemaBuilder.GetModelTransformer(field.FieldType);
					if (relatedTypeTransformer == null)
					{
						ModelEmbeddedObject<TData>(field, options, $"{sqlColumnName}_");
						return;
					}
					ModelSingleObjectRelationship(field, options, sqlColumnName, relatedTypeTransformer);
					return;
				}
				else
				{
					var relatedTypeTransformer = _schemaBuilder.GetModelTransformer(field.ElementType);
					if (relatedTypeTransformer == null)
					{
						//  todo: decide if I want to handle this use case or not
					}
					else
					{
						ModelManyObjectRelationship<TData>(field, options, sqlColumnName, relatedTypeTransformer);
					}
				}
			}
			finally
			{
				_pathStack.Pop();
			}
		}

		public void VisitModel<TField>(IModel<TField> model) where TField : IField
		{
			if (string.IsNullOrWhiteSpace(_entityTableName))
			{
				if (model is TypeModel typeModel)
					_entityTableName = typeModel.Type.Name;
				else
					throw new Exception("Table names can only be derived from TypeModels.");
			}
		}

		public EntityModel<T> GetEntityModel()
		{
			return new EntityModel<T>(_entityFields.Values.ToArray(),
				new Table(_schemaOptions.ConfiguredTableName ?? _entityTableName, _entityColumns));
		}
	}
}
