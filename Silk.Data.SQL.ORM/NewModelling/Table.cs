using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Queries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.NewModelling
{
	/// <summary>
	/// A table in a data domain.
	/// </summary>
	public class Table
	{
		public string TableName { get; }
		public bool IsEntityTable { get; }
		public Type EntityType { get; }
		public IDataField[] Fields { get; }

		public Table(string tableName, bool isEntityTable, IDataField[] dataFields, Type entityType)
		{
			if (isEntityTable && entityType == null)
				throw new ArgumentNullException(nameof(entityType), "Entity tables must specify an entity type.");

			TableName = tableName;
			IsEntityTable = isEntityTable;
			EntityType = entityType;
			Fields = dataFields;
		}

		/// <summary>
		/// Creates an <see cref="ORMQuery"/> that will create the table when executed.
		/// </summary>
		/// <returns></returns>
		public ORMQuery CreateTable()
		{
			var queryExpression = new CompositeQueryExpression();
			var columnDefinitions = new List<ColumnDefinitionExpression>();
			foreach (var field in Fields)
			{
				if (field.IsMappedObject)
					continue;

				var autoIncrement = false;
				if (field.AutoGenerate && (field.ClrType == typeof(short) ||
					field.ClrType == typeof(int) ||
					field.ClrType == typeof(long)))
					autoIncrement = true;

				columnDefinitions.Add(
					QueryExpression.DefineColumn(field.Name, field.SqlType, field.IsNullable, autoIncrement, field.IsPrimaryKey)
					);
			}
			queryExpression.Queries.Add(QueryExpression.CreateTable(TableName, columnDefinitions.ToArray()));

			foreach (var field in Fields.Where(q => q.IsIndex))
			{
				queryExpression.Queries.Add(
					QueryExpression.CreateIndex(TableName, uniqueConstraint: field.IsUnique, columns: field.Name)
					);
			}

			return new NoResultORMQuery(queryExpression);
		}

		/// <summary>
		/// Creates an <see cref="ORMQuery"/> that will drop the table when executed.
		/// </summary>
		/// <returns></returns>
		public ORMQuery DropTable()
		{
			return new NoResultORMQuery(
				QueryExpression.DropTable(TableName)
				);
		}

		/// <summary>
		/// Creates an <see cref="ORMQuery"/> that will test if the table exists when executed.
		/// </summary>
		/// <returns></returns>
		public ORMQuery TableExists()
		{
			return new ScalarResultORMQuery<int>(
				QueryExpression.TableExists(TableName)
				);
		}
	}
}
