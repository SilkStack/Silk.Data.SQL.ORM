using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class EntitySchemaOptions
	{
		protected readonly List<EntityFieldOptions> _fieldOptions =
			new List<EntityFieldOptions>();

		public EntityModelTransformer ModelTransformer { get; protected set; }
		public string ConfiguredTableName { get; protected set; }

		public abstract bool PerformTransformationPass();

		public abstract EntityModel GetEntityModel();

		public EntityFieldOptions GetFieldOptions(string[] path)
		{
			return _fieldOptions.FirstOrDefault(q => q.Path.SequenceEqual(path));
		}
	}

	public class EntitySchemaOptions<T> : EntitySchemaOptions
	{
		public TypeModel<T> EntityTypeModel { get; }

		public new EntityModelTransformer<T> ModelTransformer { get; }

		public EntitySchemaOptions(SchemaBuilder schemaBuilder)
		{
			EntityTypeModel = TypeModel.GetModelOf<T>();
			ModelTransformer = new EntityModelTransformer<T>(this, schemaBuilder);
			base.ModelTransformer = ModelTransformer;
		}

		public EntitySchemaOptions<T> TableName(string tableName)
		{
			ConfiguredTableName = tableName;
			return this;
		}

		public EntityFieldOptions<TProperty> For<TProperty>(Expression<Func<T,TProperty>> propertySelector)
		{
			if (propertySelector.Body is MemberExpression memberExpression)
			{
				var path = new List<string>();
				PopulatePath(propertySelector.Body, path);

				var fieldOptions = _fieldOptions.OfType<EntityFieldOptions<TProperty>>()
					.FirstOrDefault(q => q.Path.SequenceEqual(path));
				if (fieldOptions != null)
					return fieldOptions;

				var field = GetField(path);
				if (field == null)
					throw new ArgumentException("Field selector expression doesn't specify a valid member.", nameof(propertySelector));

				fieldOptions = new EntityFieldOptions<TProperty>(field, path);
				_fieldOptions.Add(fieldOptions);
				return fieldOptions;
			}
			throw new ArgumentException("Field selector must be a MemberExpression.", nameof(propertySelector));
		}

		private IField GetField(IEnumerable<string> path)
		{
			var fields = EntityTypeModel.Fields;
			var field = default(IField);
			foreach (var segment in path)
			{
				field = fields.FirstOrDefault(q => q.FieldName == segment);
				fields = field.FieldTypeModel?.Fields;
			}
			return field;
		}

		private void PopulatePath(Expression expression, List<string> path)
		{
			if (expression is MemberExpression memberExpression)
			{
				var parentExpr = memberExpression.Expression;
				PopulatePath(parentExpr, path);

				path.Add(memberExpression.Member.Name);
			}
		}

		public override bool PerformTransformationPass()
		{
			ModelTransformer.FieldsAdded = false;
			EntityTypeModel.Transform(ModelTransformer);
			return ModelTransformer.FieldsAdded;
		}

		public override EntityModel GetEntityModel()
		{
			return ModelTransformer.GetEntityModel();
		}
	}

	public abstract class EntityFieldOptions
	{
		public string[] Path { get; protected set; }
		public IField TypeModelField { get; protected set; }

		public string ConfiguredColumnName { get; protected set; }
		public bool IsPrimaryKey { get; protected set; }
		public bool IsAutoGenerate { get; protected set; }
		public bool IsIndex { get; protected set; }
		public string IndexName { get; protected set; }
		public IndexOption IndexOption { get; protected set; }
		public int? ConfiguredPrecision { get; protected set; }
		public int? ConfiguredScale { get; protected set; }
		public int? ConfiguredDataLength { get; protected set; }
		public SqlDataType ConfiguredDataType { get; protected set; }
	}

	public class EntityFieldOptions<T> : EntityFieldOptions
	{
		public EntityFieldOptions(IField field, IEnumerable<string> path)
		{
			TypeModelField = field;
			Path = path.ToArray();
		}

		public EntityFieldOptions<T> Index(string name = null, IndexOption indexOption = IndexOption.None)
		{
			IsIndex = true;
			IndexName = name;
			IndexOption = indexOption;
			return this;
		}

		public EntityFieldOptions<T> ColumnName(string columnName)
		{
			ConfiguredColumnName = columnName;
			return this;
		}

		public EntityFieldOptions<T> PrimaryKey(bool autoGenerate = false)
		{
			IsPrimaryKey = true;
			IsAutoGenerate = autoGenerate;
			return this;
		}

		public EntityFieldOptions<T> Precision(int precision)
		{
			ConfiguredPrecision = precision;
			return this;
		}

		public EntityFieldOptions<T> Precision(int precision, int scale)
		{
			ConfiguredPrecision = precision;
			ConfiguredScale = scale;
			return this;
		}

		public EntityFieldOptions<T> Length(int length)
		{
			ConfiguredDataLength = length;
			return this;
		}

		public EntityFieldOptions<T> DataType(SqlDataType sqlDataType)
		{
			ConfiguredDataType = sqlDataType;
			return this;
		}
	}
}
