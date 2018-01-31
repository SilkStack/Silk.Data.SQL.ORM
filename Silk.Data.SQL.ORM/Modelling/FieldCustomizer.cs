using Silk.Data.Modelling;
using System;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	/// <summary>
	/// Customizes how an entity field will be modelled.
	/// </summary>
	public abstract class FieldCustomizer
	{
		private static Type[] _autoGenerateTypes = new[]
		{
			typeof(Guid),
			typeof(sbyte),
			typeof(byte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong)
		};

		/// <summary>
		/// Gets the model field being customized.
		/// </summary>
		public abstract ModelField ModelField { get; }

		/// <summary>
		/// Gets a value indicating if the field is capable of being automatically generated.
		/// </summary>
		public bool CanAutoGenerate { get; }

		public FieldCustomizer(Type dataType)
		{
			CanAutoGenerate = _autoGenerateTypes.Contains(dataType);
		}

		/// <summary>
		/// Gets field customizations as a <see cref="FieldOpinions"/>.
		/// </summary>
		/// <returns></returns>
		public abstract FieldOpinions GetFieldOpinions();
	}

	/// <summary>
	/// Customizes how an entity field will be modelled.
	/// </summary>
	public class FieldCustomizer<TField> : FieldCustomizer
	{
		public override ModelField ModelField { get; }

		private bool _isPrimaryKey;
		private bool _autoGenerate;
		private bool _index;
		private bool _uniqueIndex;
		private SqlDataType _sqlDataType;
		private int? _dataLength;

		public FieldCustomizer(ModelField modelField) :
			base(modelField.DataType)
		{
			ModelField = modelField;
		}

		/// <summary>
		/// Get a FieldOptions instance of the customized field options.
		/// </summary>
		/// <returns></returns>
		public override FieldOpinions GetFieldOpinions()
		{
			return new FieldOpinions(
				_sqlDataType, _dataLength, _isPrimaryKey, _autoGenerate,
				_index, _uniqueIndex
				);
		}

		/// <summary>
		/// Use this field as a primary key.
		/// </summary>
		/// <param name="autoGenerate">Specifies if the field should be auto generated. Supported for integers and Guid types.</param>
		/// <returns></returns>
		public FieldCustomizer<TField> IsPrimaryKey(bool autoGenerate = false)
		{
			if (autoGenerate && !CanAutoGenerate)
				throw new InvalidOperationException("Field can not be set to auto generate.");

			_autoGenerate = autoGenerate;
			_isPrimaryKey = true;
			return this;
		}

		/// <summary>
		/// Make an index on the field.
		/// </summary>
		/// <param name="requireUnique">Set to true to create a unique constraint on the field.</param>
		/// <returns></returns>
		public FieldCustomizer<TField> Index(bool requireUnique = false)
		{
			_index = true;
			_uniqueIndex = requireUnique;
			return this;
		}

		/// <summary>
		/// Set the data length for the field.
		/// </summary>
		/// <param name="length"></param>
		/// <returns></returns>
		public FieldCustomizer<TField> Length(int length)
		{
			_dataLength = length;
			return this;
		}

		/// <summary>
		/// Sets the specific SQL datatype to use to store the field.
		/// </summary>
		/// <param name="dataType"></param>
		/// <returns></returns>
		public FieldCustomizer<TField> SqlType(SqlDataType dataType)
		{
			_sqlDataType = dataType;
			return this;
		}
	}
}
