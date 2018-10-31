using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Represents an entity schema in the process of being assembled.
	/// </summary>
	public interface IEntitySchemaAssemblage
	{
		IEntitySchemaBuilder Builder { get; }
		IEntitySchemaDefinition Definition { get; }

		string TableName { get; }
		IReadOnlyCollection<Column> Columns { get; }

		IReadOnlyCollection<IEntityFieldAssemblage> Fields { get; }

		void AddField(IEntityFieldAssemblage fieldAssemblage);
	}

	/// <summary>
	/// Represents an entity schema for type T in the process of being assembled.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EntitySchemaAssemblage<T> : IEntitySchemaAssemblage
		where T : class
	{
		private readonly List<IEntityFieldAssemblage> _fields
			= new List<IEntityFieldAssemblage>();
		private readonly List<Column> _columns
			= new List<Column>();

		public string TableName { get; }
		public IReadOnlyCollection<IEntityFieldAssemblage> Fields => _fields;

		public IEntitySchemaBuilder Builder { get; }

		public IEntitySchemaDefinition Definition { get; }

		public IReadOnlyCollection<Column> Columns => _columns;

		public EntitySchemaAssemblage(string tableName,
			IEntitySchemaDefinition definition, IEntitySchemaBuilder builder)
		{
			TableName = tableName;
			Builder = builder;
			Definition = definition;
		}

		public void AddField(IEntityFieldAssemblage fieldAssemblage)
		{
			_fields.Add(fieldAssemblage);
			AddColumns(fieldAssemblage);
		}

		private void AddColumns(IEntityFieldAssemblage fieldAssemblage)
		{
			_columns.Add(new Column(
				fieldAssemblage.FieldDefinition.ColumnName,
				fieldAssemblage.FieldDefinition.SqlDataType,
				fieldAssemblage.FieldDefinition.IsNullable
				));
		}
	}
}
