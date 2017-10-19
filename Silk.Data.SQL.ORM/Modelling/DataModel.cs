using Silk.Data.Modelling;
using Silk.Data.Modelling.ResourceLoaders;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Silk.Data.SQL.ORM.Modelling
{
	public abstract class DataModel : IView<DataField>
	{
		public DataField[] Fields { get; }

		public string Name { get; }

		public Model Model { get; }

		public IResourceLoader[] ResourceLoaders { get; }

		IViewField[] IView.Fields => Fields;

		public TableSchema[] Tables { get; }

		public DataModel(string name, Model model, DataField[] fields,
			IResourceLoader[] resourceLoaders)
		{
			Name = name;
			Model = model;
			Fields = fields;
			ResourceLoaders = resourceLoaders;
			Tables = Fields.Select(q => q.Storage.Table).GroupBy(q => q)
				.Select(q => q.First()).ToArray();
		}
	}

	public class DataModel<TSource, TView> : DataModel, IView<DataField, TSource, TView>
		where TSource : new()
		where TView: new()
	{
		public new TypedModel<TSource> Model { get; }

		public DataModel(string name, TypedModel<TSource> model, DataField[] fields, IResourceLoader[] resourceLoaders)
			: base(name, model, fields, resourceLoaders)
		{
			Model = model;
		}

		public void Insert(IDataProvider dataProvider, params TSource[] sources)
		{
			//  todo: update this to work with datamodels that span multiple tables
			var table = Fields.First().Storage.Table;
			var queries = new List<QueryExpression>();
			var columns = Fields.Where(
				dataField => !dataField.Storage.IsAutoIncrement
				).ToArray();

			GenerateIds(sources);

			var views = this.MapToViewAsync(sources).ConfigureAwait(false)
				.GetAwaiter().GetResult();

			var autoIncField = Fields.FirstOrDefault(q => q.Storage.IsAutoIncrement);

			if (autoIncField == null)
			{
				queries.Add(BulkInsertExpression(views, columns, table));
				dataProvider.ExecuteNonQuery(QueryExpression.Transaction(queries));
				return;
			}

			var viewContainer = new ObjectContainer<TView>(Model, this);
			foreach (var view in views)
			{
				viewContainer.Instance = view;
				var expressionTuple = InsertAndGetIdExpression(columns, table, viewContainer);
				queries.Add(expressionTuple.insert);
				queries.Add(expressionTuple.getId);
			}
			var ids = new List<object>();
			using (var queryResult = dataProvider.ExecuteReader(QueryExpression.Transaction(queries)))
			{
				while (queryResult.NextResult())
				{
					if (queryResult.Read())
					{
						ids.Add(queryResult.GetInt32(0));
					}
				}
			}
			AssignIds(sources, ids, autoIncField);
		}

		public async Task InsertAsync(IDataProvider dataProvider, params TSource[] sources)
		{
			//  todo: update this to work with datamodels that span multiple tables
			var table = Fields.First().Storage.Table;
			var queries = new List<QueryExpression>();
			var columns = Fields.Where(
				dataField => !dataField.Storage.IsAutoIncrement
				).ToArray();

			GenerateIds(sources);

			var views = await this.MapToViewAsync(sources)
				.ConfigureAwait(false);

			var autoIncField = Fields.FirstOrDefault(q => q.Storage.IsAutoIncrement);

			if (autoIncField == null)
			{
				queries.Add(BulkInsertExpression(views, columns, table));
				await dataProvider.ExecuteNonQueryAsync(QueryExpression.Transaction(queries))
					.ConfigureAwait(false);
				return;
			}

			var viewContainer = new ObjectContainer<TView>(Model, this);
			foreach (var view in views)
			{
				viewContainer.Instance = view;
				var expressionTuple = InsertAndGetIdExpression(columns, table, viewContainer);
				queries.Add(expressionTuple.insert);
				queries.Add(expressionTuple.getId);
			}
			var ids = new List<object>();
			using (var queryResult = await dataProvider.ExecuteReaderAsync(QueryExpression.Transaction(queries))
				.ConfigureAwait(false))
			{
				while (queryResult.NextResult())
				{
					if (queryResult.Read())
					{
						ids.Add(queryResult.GetInt32(0));
					}
				}
			}
			AssignIds(sources, ids, autoIncField);
		}

		private (QueryExpression insert, QueryExpression getId) InsertAndGetIdExpression(DataField[] columns,
			TableSchema table, ObjectContainer<TView> viewContainer)
		{
			var row = new QueryExpression[columns.Length];
			for (var i = 0; i < columns.Length; i++)
			{
				row[i] = QueryExpression.Value(viewContainer.GetValue(
					columns[i].ModelBinding.ViewFieldPath
					));
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

		private QueryExpression BulkInsertExpression(TView[] views, DataField[] columns, TableSchema table)
		{
			var values = new List<QueryExpression[]>();
			var viewContainer = new ObjectContainer<TView>(Model, this);
			foreach (var view in views)
			{
				viewContainer.Instance = view;
				var row = new QueryExpression[columns.Length];
				for (var i = 0; i < columns.Length; i++)
				{
					row[i] = QueryExpression.Value(viewContainer.GetValue(
						columns[i].ModelBinding.ViewFieldPath
						));
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

		private void AssignIds(TSource[] sources, List<object> ids, DataField autoIncField)
		{
			var autoIncModelField = Model.GetField(autoIncField.ModelBinding.ModelFieldPath);
			var modelWriter = new ObjectReadWriter(typeof(TSource), Model, null);

			for(var i = 0; i < sources.Length; i++)
			{
				modelWriter.Value = sources[i];
				modelWriter.GetField(autoIncModelField).Value = ids[i];
			}
		}

		private void GenerateIds(TSource[] sources)
		{
			var autoGenerateField = Fields.FirstOrDefault(q => q.Storage.IsAutoGenerate &&
				q.DataType == typeof(Guid));

			if (autoGenerateField != null)
			{
				var autoGenerateModelField = Model.GetField(autoGenerateField.ModelBinding.ModelFieldPath);
				var modelWriter = new ObjectReadWriter(typeof(TSource), Model, null);

				foreach (var source in sources)
				{
					modelWriter.Value = source;
					modelWriter.GetField(autoGenerateModelField).Value = Guid.NewGuid();
				}
			}
		}
	}
}
