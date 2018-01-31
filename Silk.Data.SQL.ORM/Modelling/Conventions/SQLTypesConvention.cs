using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using System;
using System.Linq;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Modelling.Conventions
{
	/// <summary>
	/// Models supported SQL types.
	/// </summary>
	public class SQLTypesConvention : ISchemaConvention
	{
		public void VisitModel(TypedModel model, SchemaBuilder builder)
		{
			//  CanWrite is optional here, when CanWrite is false the computed value will be stored with the entity
			foreach (var field in model.Fields.Where(q => q.CanRead))
			{
				var sqlDataType = GetSqlDataType(field, builder);
				if (sqlDataType == null)
					continue;

				if (builder.IsFieldDefined(model, field.Name))
					continue;

				var bindingDirection = field.CanWrite ? BindingDirection.Bidirectional : BindingDirection.ModelToView;

				builder.DefineField(
					model, field.Name, sqlDataType,
					new AssignmentBinding(bindingDirection, new[] { field.Name }, new[] { field.Name })
					);
			}
		}

		private SqlDataType GetSqlDataType(ModelField modelField, SchemaBuilder builder)
		{
			var fieldOpinions = builder.GetFieldOpinions(modelField);
			if (fieldOpinions.DataType != null)
				return fieldOpinions.DataType;

			var dataType = modelField.DataType;
			if (dataType == typeof(string))
			{
				if (fieldOpinions.DataLength != null)
					return SqlDataType.Text(fieldOpinions.DataLength.Value);
				return SqlDataType.Text();
			}
			else if (dataType == typeof(bool))
			{
				return SqlDataType.Bit();
			}
			else if (dataType == typeof(sbyte))
			{
				return SqlDataType.TinyInt();
			}
			else if (dataType == typeof(byte))
			{
				return SqlDataType.UnsignedTinyInt();
			}
			else if (dataType == typeof(short))
			{
				return SqlDataType.SmallInt();
			}
			else if (dataType == typeof(ushort))
			{
				return SqlDataType.UnsignedSmallInt();
			}
			else if (dataType == typeof(int))
			{
				return SqlDataType.Int();
			}
			else if (dataType == typeof(uint))
			{
				return SqlDataType.UnsignedInt();
			}
			else if (dataType == typeof(long))
			{
				return SqlDataType.BigInt();
			}
			else if (dataType == typeof(ulong))
			{
				return SqlDataType.UnsignedBigInt();
			}
			else if (dataType == typeof(float))
			{
				if (fieldOpinions.Precision != null)
					return SqlDataType.Float(fieldOpinions.Precision.Value);
				return SqlDataType.Float(SqlDataType.FLOAT_MAX_PRECISION);
			}
			else if (dataType == typeof(double))
			{
				if (fieldOpinions.Precision != null)
					return SqlDataType.Float(fieldOpinions.Precision.Value);
				return SqlDataType.Float(SqlDataType.DOUBLE_MAX_PRECISION);
			}
			else if (dataType == typeof(decimal))
			{
				if (fieldOpinions.Precision != null && fieldOpinions.Scale != null)
					return SqlDataType.Decimal(fieldOpinions.Precision.Value, fieldOpinions.Scale.Value);
				else if (fieldOpinions.Precision != null)
					return SqlDataType.Decimal(fieldOpinions.Precision.Value);
				return SqlDataType.Decimal();
			}
			else if (dataType == typeof(Guid))
			{
				return SqlDataType.Guid();
			}
			else if (dataType == typeof(byte[]))
			{
				if (fieldOpinions.DataLength == null)
					throw new InvalidOperationException("A data length is required for binary blob storage. Use the field customizer API to specify a data length.");
				return SqlDataType.Binary(fieldOpinions.DataLength.Value);
			}
			else if (dataType == typeof(DateTime))
			{
				return SqlDataType.DateTime();
			}
			else if (dataType.GetTypeInfo().IsEnum)
			{
				return SqlDataType.Int();
			}

			return null;
		}
	}
}
