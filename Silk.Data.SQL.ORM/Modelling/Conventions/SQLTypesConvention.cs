using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using System;
using System.Linq;

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
			else if (dataType == typeof(byte))
			{
				return SqlDataType.TinyInt();
			}
			else if (dataType == typeof(short))
			{
				return SqlDataType.SmallInt();
			}
			else if (dataType == typeof(int))
			{
				return SqlDataType.Int();
			}
			else if (dataType == typeof(long))
			{
				return SqlDataType.BigInt();
			}
			else if (dataType == typeof(float))
			{
				return SqlDataType.Float(SqlDataType.FLOAT_MAX_PRECISION);
			}
			else if (dataType == typeof(double))
			{
				return SqlDataType.Float(SqlDataType.DOUBLE_MAX_PRECISION);
			}
			else if (dataType == typeof(decimal))
			{
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

			return null;
		}
	}
}
