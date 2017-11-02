using System.Collections.Generic;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class EntitySchema
	{
		private readonly List<Table> _tables = new List<Table>();
		private readonly List<Table> _relationshipTables = new List<Table>();

		public EntityModel EntityModel { get; private set; }
		public IReadOnlyList<Table> Tables => _tables;
		public IReadOnlyList<Table> RelationshipTables => _relationshipTables;
		public Table EntityTable { get; private set; }

		internal EntitySchema() { }

		public EntitySchema(EntityModel entityModel, Table entityTable, Table[] relationshipTables)
		{
			EntityModel = entityModel;
			EntityTable = entityTable;
			_relationshipTables.AddRange(relationshipTables);
			_tables.Add(entityTable);
			_tables.AddRange(relationshipTables);
		}

		internal void AddTable(Table table)
		{
			if (table.IsEntityTable)
			{
				EntityTable = table;
			}
			else
			{
				_relationshipTables.Add(table);
			}
			_tables.Add(table);
		}

		internal void SetEntityModel(EntityModel entityModel)
		{
			EntityModel = entityModel;
		}
	}
}
