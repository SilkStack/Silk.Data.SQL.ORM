using System;
using System.Collections.Generic;
using System.Linq;
using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Schema;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class EntityModelTransformer<T> : IModelTransformer
	{
		public bool FieldsAdded { get; set; }

		private string _entityTableName;
		private readonly List<Column> _entityColumns =
			new List<Column>();
		private readonly Dictionary<string, IEntityField> _entityFields =
			new Dictionary<string, IEntityField>();
		private readonly EntitySchemaOptions<T> _schemaOptions;

		public EntityModelTransformer(EntitySchemaOptions<T> schemaOptions)
		{
			_schemaOptions = schemaOptions;
		}

		public void VisitField<TData>(IField<TData> field)
		{
			if (!field.CanRead)
				return;

			var options = _schemaOptions.GetFieldOptions(field);
			var sqlColumnName = options?.ConfiguredColumnName;
			if (string.IsNullOrWhiteSpace(sqlColumnName))
				sqlColumnName = field.FieldName;

			if (_entityFields.ContainsKey(field.FieldName))
				return;

			if (SqlDataTypes.IsSQLPrimitiveType(field.FieldType))
			{
				var column = new Column(sqlColumnName, SqlDataTypes.GetSqlDataType(field, options));

				_entityColumns.Add(column);
				_entityFields.Add(field.FieldName,
					new ValueField(field.FieldName, field.CanRead, field.CanWrite, false, null, column)
					);
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
				new Table(_entityTableName, _entityColumns));
		}
	}
}
