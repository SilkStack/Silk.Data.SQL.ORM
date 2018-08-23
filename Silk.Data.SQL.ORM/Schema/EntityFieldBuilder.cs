using Silk.Data.Modelling;
using System;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Configures and builds an entity field.
	/// </summary>
	public abstract class EntityFieldBuilder
	{
		/// <summary>
		/// Gets the model field the field builder represents.
		/// </summary>
		public abstract IPropertyField ModelField { get; }

		/// <summary>
		/// Gets or sets the column name to use to store the field.
		/// </summary>
		public string ColumnName { get; set; }

		/// <summary>
		/// Gets or sets if the column is nullable.
		/// </summary>
		public bool IsNullable { get; set; }

		/// <summary>
		/// Gets or sets if the column is a primary key.
		/// </summary>
		public bool IsPrimaryKey { get; set; }

		/// <summary>
		/// Gets or sets the datatype to store the field as.
		/// </summary>
		public SqlDataType SqlDataType { get; set; }

		/// <summary>
		/// Builds the entity field.
		/// </summary>
		/// <returns>Null when the field shouldn't be stored in the schema being built.</returns>
		public abstract EntityField Build(string columnNamePrefix);
	}

	/// <summary>
	/// Configures and builds an entity field of type T.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EntityFieldBuilder<T> : EntityFieldBuilder
	{
		/// <summary>
		/// Gets the model field the field builder represents.
		/// </summary>
		public override IPropertyField ModelField { get; }

		public EntityFieldBuilder(IPropertyField modelField)
		{
			ModelField = modelField;
			ColumnName = modelField.FieldName;
			if (SqlTypeHelper.IsSqlPrimitiveType(typeof(T)))
			{
				SqlDataType = SqlTypeHelper.GetDataType(typeof(T));
				IsNullable = TypeIsNullable(typeof(T));
			}
			else
			{
				SqlDataType = SqlTypeHelper.GetDataType(typeof(bool));
				IsNullable = false;
			}

			if (modelField.FieldName == "Id")
				IsPrimaryKey = true;
		}

		public override EntityField Build(string columnNamePrefix)
		{
			if (SqlDataType == null || !ModelField.CanRead || ModelField.IsEnumerable)
				return null;

			var primaryKeyGenerator = PrimaryKeyGenerator.NotPrimaryKey;
			if (IsPrimaryKey)
				primaryKeyGenerator = GetPrimaryKeyGenerator(SqlDataType);
			return new EntityField<T>(new[] { new Column($"{columnNamePrefix}{ColumnName}", SqlDataType, IsNullable) },
				ModelField, primaryKeyGenerator);
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

		private static bool TypeIsNullable(Type type)
		{
			if (type == typeof(string))
				return true;
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				return true;
			return false;
		}
	}
}
