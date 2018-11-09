using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Represents an entity schema in the process of being assembled.
	/// </summary>
	public interface IEntitySchemaAssemblage
	{
		Type EntityType { get; }
		IEntitySchemaBuilder Builder { get; }
		IEntitySchemaDefinition Definition { get; }

		string TableName { get; }
		IReadOnlyCollection<Column> Columns { get; }

		IReadOnlyCollection<ISchemaFieldAssemblage> Fields { get; }

		void AddField(ISchemaFieldAssemblage fieldAssemblage);
	}

	/// <summary>
	/// Represents an entity schema for type T in the process of being assembled.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EntitySchemaAssemblage<T> : IEntitySchemaAssemblage
		where T : class
	{
		private readonly List<ISchemaFieldAssemblage> _fields
			= new List<ISchemaFieldAssemblage>();
		private readonly List<Column> _columns
			= new List<Column>();

		public Type EntityType { get; } = typeof(T);
		public string TableName { get; }
		public IReadOnlyCollection<ISchemaFieldAssemblage> Fields => _fields;

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

		public void AddField(ISchemaFieldAssemblage fieldAssemblage)
		{
			_fields.Add(fieldAssemblage);
			AddColumns(fieldAssemblage);
		}

		private void AddColumns(ISchemaFieldAssemblage fieldAssemblage)
		{
			_columns.Add(new Column(
				fieldAssemblage.FieldDefinition.ColumnName,
				fieldAssemblage.FieldDefinition.SqlDataType,
				fieldAssemblage.FieldDefinition.IsNullable
				));
		}
	}
}
