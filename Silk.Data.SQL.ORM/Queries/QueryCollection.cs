using Silk.Data.Modelling;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Queries
{
	public abstract class QueryCollection
	{
		protected List<QueryWithDelegate> Queries { get; } = new List<QueryWithDelegate>();
	}

	public class ExecutableQueryCollection : QueryCollection
	{
		public ExecutableQueryCollection(IEnumerable<QueryWithDelegate> queryExpressions)
		{
			Queries.AddRange(queryExpressions);
		}

		public ExecutableQueryCollection(params QueryWithDelegate[] queryExpressions)
		{
			Queries.AddRange(queryExpressions);
		}

		public ExecutableQueryCollection AsTransaction()
		{
			return new ExecutableQueryCollection(new QueryWithDelegate(
				QueryExpression.Transaction(Queries.Select(q => q.Query)),
				queryResult =>
				{
					foreach (var query in Queries)
					{
						if (query.Delegate != null)
						{
							if (!queryResult.NextResult())
								throw new Exception("Failed to move to query result.");
							query.Delegate(queryResult);
						}
					}
				},
				async queryResult =>
				{
					foreach (var query in Queries)
					{
						if (query.AsyncDelegate != null)
						{
							if (!await queryResult.NextResultAsync().ConfigureAwait(false))
								throw new Exception("Failed to move to query result.");
							await query.AsyncDelegate(queryResult).ConfigureAwait(false);
						}
					}
				}));
		}

		public void Execute(IDataProvider dataProvider)
		{
			foreach (var query in Queries)
			{
				if (query.Delegate == null)
				{
					dataProvider.ExecuteNonQuery(query.Query);
				}
				else
				{
					using (var queryResult = dataProvider.ExecuteReader(query.Query))
					{
						query.Delegate(queryResult);
					}
				}
			}
		}

		public async Task ExecuteAsync(IDataProvider dataProvider)
		{
			foreach (var query in Queries)
			{
				if (query.Delegate == null)
				{
					await dataProvider.ExecuteNonQueryAsync(query.Query)
						.ConfigureAwait(false);
				}
				else
				{
					using (var queryResult = await dataProvider.ExecuteReaderAsync(query.Query)
						.ConfigureAwait(false))
					{
						await query.AsyncDelegate(queryResult)
							.ConfigureAwait(false);
					}
				}
			}
		}
	}

	public class ModelBoundExecutableQueryCollection<TSource> : ExecutableQueryCollection
		where TSource : new()
	{
		public DataModel<TSource> DataModel { get; }

		public ModelBoundExecutableQueryCollection(DataModel<TSource> dataModel, params QueryWithDelegate[] queryExpressions)
			: base(queryExpressions)
		{
			DataModel = dataModel;
		}

		public ModelBoundExecutableQueryCollection(DataModel<TSource> dataModel, IEnumerable<QueryWithDelegate> queryExpressions)
			: base(queryExpressions)
		{
			DataModel = dataModel;
		}

		public ModelBoundExecutableQueryCollection<TSource> Insert(params TSource[] sources)
		{
			if (sources.Length < 1)
				throw new ArgumentOutOfRangeException(nameof(sources), "Must provide at least 1 source.");

			//  todo: update this to work with datamodels that span multiple tables
			var table = DataModel.Fields.First().Storage.Table;
			var queries = new List<QueryWithDelegate>(Queries);
			var columns = DataModel.Fields.Where(
				dataField => !dataField.Storage.IsAutoIncrement
				).ToArray();

			var modelReaders = sources.Select(source => new ObjectReadWriter(typeof(TSource), DataModel.Model, source)).ToArray();
			var viewContainers = sources.Select(source => new InsertContainer(DataModel.Model, DataModel)).ToArray();

			GenerateIds(modelReaders);

			//  todo: replace this with a non-async map method built for datamodels specifically
			//		this will NOT support loading resources when mapping TO views
			DataModel.MapToViewAsync(modelReaders, viewContainers)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();

			var autoIncField = DataModel.Fields.FirstOrDefault(q => q.Storage.IsAutoIncrement);

			if (autoIncField == null)
			{
				queries.Add(new QueryWithDelegate(
					BulkInsertExpression(columns, table, viewContainers)
					));
				return new ModelBoundExecutableQueryCollection<TSource>(DataModel, queries);
			}

			var autoIncModelField = DataModel.Model.GetField(autoIncField.ModelBinding.ModelFieldPath);
			var modelWriter = new ObjectReadWriter(typeof(TSource), DataModel.Model, null);
			var i = -1;
			foreach (var viewContainer in viewContainers)
			{
				var expressionTuple = InsertAndGetIdExpression(columns, table, viewContainer);
				queries.Add(new QueryWithDelegate(expressionTuple.insert));
				queries.Add(new QueryWithDelegate(expressionTuple.getId, queryResult => {
					i++;
					if (queryResult.Read())
					{
						if (autoIncField.DataType == typeof(short))
							modelReaders[i].GetField(autoIncModelField).Value = queryResult.GetInt16(0);
						else if (autoIncField.DataType == typeof(long))
							modelReaders[i].GetField(autoIncModelField).Value = queryResult.GetInt64(0);
						else
							modelReaders[i].GetField(autoIncModelField).Value = queryResult.GetInt32(0);
					}
					else
						throw new Exception("Failed to get auto generated ID.");
				}, async queryResult => {
					i++;
					if (await queryResult.ReadAsync().ConfigureAwait(false))
					{
						if (autoIncField.DataType == typeof(short))
							modelReaders[i].GetField(autoIncModelField).Value = queryResult.GetInt16(0);
						else if (autoIncField.DataType == typeof(long))
							modelReaders[i].GetField(autoIncModelField).Value = queryResult.GetInt64(0);
						else
							modelReaders[i].GetField(autoIncModelField).Value = queryResult.GetInt32(0);
					}
					else
						throw new Exception("Failed to get auto generated ID.");
				}));
			}

			return new ModelBoundExecutableQueryCollection<TSource>(DataModel, queries);
		}

		private (QueryExpression insert, QueryExpression getId) InsertAndGetIdExpression(DataField[] columns,
			TableSchema table, InsertContainer viewContainer)
		{
			var row = new QueryExpression[columns.Length];
			for (var i = 0; i < columns.Length; i++)
			{
				if (!viewContainer.ColumnValues.TryGetValue(columns[i].Storage.ColumnName, out row[i]))
				{
					row[i] = QueryExpression.Value(null);
				}
			}

			return (QueryExpression.Insert(table.TableName,
					columns.Select(
						dataField => dataField.Name
					).ToArray(),
					row
					),
					QueryExpression.Select(new[] {
						QueryExpression.LastInsertIdFunction()
					}));
		}

		private QueryExpression BulkInsertExpression(DataField[] columns, TableSchema table,
			InsertContainer[] insertContainers)
		{
			var values = new List<QueryExpression[]>();
			foreach (var container in insertContainers)
			{
				var row = new QueryExpression[columns.Length];
				for (var i = 0; i < columns.Length; i++)
				{
					if (!container.ColumnValues.TryGetValue(columns[i].Storage.ColumnName, out row[i]))
					{
						row[i] = QueryExpression.Value(null);
					}
				}
				values.Add(row);
			}
			return QueryExpression.Insert(table.TableName,
					columns.Select(
						dataField => dataField.Name
					).ToArray(),
					values.ToArray()
					);
		}

		private void GenerateIds(IModelReadWriter[] modelReaders)
		{
			var autoGenerateField = DataModel.Fields.FirstOrDefault(q => q.Storage.IsAutoGenerate &&
				q.DataType == typeof(Guid));

			if (autoGenerateField != null)
			{
				var autoGenerateModelField = DataModel.Model.GetField(autoGenerateField.ModelBinding.ModelFieldPath);

				foreach (var modelReader in modelReaders)
				{
					modelReader.GetField(autoGenerateModelField).Value = Guid.NewGuid();
				}
			}
		}
	}
}
