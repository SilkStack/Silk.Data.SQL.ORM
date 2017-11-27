using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Queries
{
	public class InsertQueryBuilder<TSource>
		where TSource : new()
	{
		public EntityModel<TSource> DataModel { get; }

		public InsertQueryBuilder(EntityModel<TSource> dataModel)
		{
			DataModel = dataModel;
		}

		public ICollection<ORMQuery> CreateQuery(params TSource[] sources)
		{
			if (sources == null || sources.Length < 1)
				throw new ArgumentException("At least one source must be provided.", nameof(sources));

			var queries = new List<ORMQuery>();
			var isBulkInsert = !DataModel.PrimaryKeyFields.Any(q => q.Storage.IsAutoIncrement);
			List<QueryExpression[]> rows = null;
			if (isBulkInsert)
				rows = new List<QueryExpression[]>();

			//  map sources to the datamodel view
			var sourceReadWriters = sources
				.Select(q => new ObjectModelReadWriter(DataModel.Model, q))
				.ToArray();

			foreach (var sourceReadWriter in sourceReadWriters)
			{
				var row = new List<QueryExpression>();
				foreach (var field in DataModel.Fields)
				{
					if (field.Storage.IsAutoGenerate &&
						field.DataType == typeof(Guid))
					{
						field.ModelBinding.WriteValue(sourceReadWriter, Guid.NewGuid());
					}

					row.Add(QueryExpression.Value(
						field.ModelBinding.ReadValue<object>(sourceReadWriter)
						));
				}

				if (isBulkInsert)
				{
					rows.Add(row.ToArray());
				}
				else
				{
					//  create a standalone insert
				}
			}

			if (isBulkInsert)
			{
				queries.Add(new NoResultORMQuery(
					QueryExpression.Insert(
						DataModel.Name,
						DataModel.Fields
							.Where(q => q.Storage.Table.IsEntityTable)
							.Select(q => q.Storage.ColumnName).ToArray(),
						rows.ToArray()
					)));
			}

			return queries;
		}

		//public ICollection<QueryWithDelegate> CreateQuery(params TSource[] sources)
		//{
		//	if (sources.Length < 1)
		//		throw new ArgumentOutOfRangeException(nameof(sources), "Must provide at least 1 source.");

		//	//  todo: update this to work with datamodels that span multiple tables
		//	var table = DataModel.Schema.Tables.First(q => q.IsEntityTable);
		//	var queries = new List<QueryWithDelegate>();
		//	var columns = DataModel.Fields.Where(
		//		dataField => !dataField.Storage.IsAutoIncrement
		//		).ToArray();

		//	var modelReaders = sources.Select(source => new ObjectReadWriter(typeof(TSource), DataModel.Model, source)).ToArray();
		//	var viewContainers = sources.Select(source => new InsertContainer(DataModel.Model, DataModel)).ToArray();

		//	GenerateIds(modelReaders);

		//	DataModel.MapToView(modelReaders, viewContainers);

		//	var autoIncField = DataModel.Fields.FirstOrDefault(q => q.Storage.IsAutoIncrement);

		//	if (autoIncField == null)
		//	{
		//		queries.Add(new QueryWithDelegate(
		//			BulkInsertExpression(columns, table, viewContainers)
		//			));
		//		return queries;
		//	}

		//	var autoIncModelField = DataModel.Model.GetField(autoIncField.ModelBinding.ModelFieldPath);
		//	var modelWriter = new ObjectReadWriter(typeof(TSource), DataModel.Model, null);
		//	var i = -1;
		//	foreach (var viewContainer in viewContainers)
		//	{
		//		var expressionTuple = InsertAndGetIdExpression(columns, table, viewContainer);
		//		queries.Add(new QueryWithDelegate(expressionTuple.insert));
		//		queries.Add(new QueryWithDelegate(expressionTuple.getId, queryResult => {
		//			i++;
		//			if (queryResult.Read())
		//			{
		//				if (autoIncField.DataType == typeof(short))
		//					modelReaders[i].GetField(autoIncModelField).Value = queryResult.GetInt16(0);
		//				else if (autoIncField.DataType == typeof(long))
		//					modelReaders[i].GetField(autoIncModelField).Value = queryResult.GetInt64(0);
		//				else
		//					modelReaders[i].GetField(autoIncModelField).Value = queryResult.GetInt32(0);
		//			}
		//			else
		//				throw new Exception("Failed to get auto generated ID.");
		//		}, async queryResult => {
		//			i++;
		//			if (await queryResult.ReadAsync().ConfigureAwait(false))
		//			{
		//				if (autoIncField.DataType == typeof(short))
		//					modelReaders[i].GetField(autoIncModelField).Value = queryResult.GetInt16(0);
		//				else if (autoIncField.DataType == typeof(long))
		//					modelReaders[i].GetField(autoIncModelField).Value = queryResult.GetInt64(0);
		//				else
		//					modelReaders[i].GetField(autoIncModelField).Value = queryResult.GetInt32(0);
		//			}
		//			else
		//				throw new Exception("Failed to get auto generated ID.");
		//		}));
		//	}
		//	return queries;
		//}

		//public ICollection<QueryWithDelegate> CreateQuery(params IContainer[] sources)
		//{
		//	if (sources.Length < 1)
		//		throw new ArgumentOutOfRangeException(nameof(sources), "Must provide at least 1 source.");

		//	var queryList = new List<QueryWithDelegate>();

		//	foreach (var source in sources)
		//	{
		//		var tableRows = new Dictionary<Table, List<AssignColumnExpression>>();
		//		QueryExpression selectExpression = null;
		//		foreach (var dataField in DataModel.Fields)
		//		{
		//			var table = dataField.Storage.Table;
		//			if (!tableRows.ContainsKey(table))
		//				tableRows[table] = new List<AssignColumnExpression>();

		//			var row = tableRows[table];

		//			if (dataField.Storage.IsAutoGenerate)
		//			{
		//				if (dataField.Storage.IsAutoIncrement)
		//				{
		//					//  todo: generate a SELECT for the newly created ID
		//				}
		//				else if (dataField.DataType == typeof(Guid))
		//				{
		//					var newId = Guid.NewGuid();
		//					row.Add(QueryExpression.Assign(dataField.Storage.ColumnName,
		//						newId));
		//					source.SetValue(dataField.ModelBinding.ModelFieldPath, newId);
		//				}
		//				else
		//				{
		//					throw new InvalidOperationException("Unsupported primary key configuration.");
		//				}
		//			}
		//			else
		//			{
		//				//  ViewFieldPath here doesn't work for all View->View mapping, ex. ViewFieldPath can be "OwnerId" for an "Owner"."Id" property
		//				//  ModelFieldPath isn't correct because this Insert overload is expecting a view to be passed in
		//				//  todo: resolve this conflict somehow
		//				row.Add(QueryExpression.Assign(dataField.Storage.ColumnName,
		//					source.GetValue(dataField.ModelBinding.ModelFieldPath)));
		//			}
		//		}

		//		foreach (var kvp in tableRows)
		//		{
		//			queryList.Add(new QueryWithDelegate(
		//				QueryExpression.Insert(
		//					QueryExpression.Table(kvp.Key.TableName),
		//					kvp.Value.Select(q => q.Column).ToArray(),
		//					kvp.Value.Select(q => ((ValueExpression)q.Expression).Value).ToArray()
		//				)));
		//		}
		//	}

		//	return queryList;
		//}

		//private (QueryExpression insert, QueryExpression getId) InsertAndGetIdExpression(DataField[] columns,
		//	Table table, InsertContainer viewContainer)
		//{
		//	var row = new QueryExpression[columns.Length];
		//	for (var i = 0; i < columns.Length; i++)
		//	{
		//		if (!viewContainer.ColumnValues.TryGetValue(columns[i].Storage.ColumnName, out row[i]))
		//		{
		//			row[i] = QueryExpression.Value(null);
		//		}
		//	}

		//	return (QueryExpression.Insert(table.TableName,
		//			columns.Select(
		//				dataField => dataField.Storage.ColumnName
		//			).ToArray(),
		//			row
		//			),
		//			QueryExpression.Select(new[] {
		//				QueryExpression.LastInsertIdFunction()
		//			}));
		//}

		//private QueryExpression BulkInsertExpression(DataField[] columns, Table table,
		//	InsertContainer[] insertContainers)
		//{
		//	var values = new List<QueryExpression[]>();
		//	foreach (var container in insertContainers)
		//	{
		//		var row = new QueryExpression[columns.Length];
		//		for (var i = 0; i < columns.Length; i++)
		//		{
		//			if (!container.ColumnValues.TryGetValue(columns[i].Storage.ColumnName, out row[i]))
		//			{
		//				row[i] = QueryExpression.Value(null);
		//			}
		//		}
		//		values.Add(row);
		//	}
		//	return QueryExpression.Insert(table.TableName,
		//			columns.Select(
		//				dataField => dataField.Storage.ColumnName
		//			).ToArray(),
		//			values.ToArray()
		//			);
		//}

		//private void GenerateIds(IModelReadWriter[] modelReaders)
		//{
		//	var autoGenerateField = DataModel.Fields.FirstOrDefault(q => q.Storage.IsAutoGenerate &&
		//		q.DataType == typeof(Guid));

		//	if (autoGenerateField != null)
		//	{
		//		var autoGenerateModelField = DataModel.Model.GetField(autoGenerateField.ModelBinding.ModelFieldPath);

		//		foreach (var modelReader in modelReaders)
		//		{
		//			modelReader.GetField(autoGenerateModelField).Value = Guid.NewGuid();
		//		}
		//	}
		//}
	}
}
