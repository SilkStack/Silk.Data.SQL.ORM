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

		public DataDomain Domain { get; }

		public DataField[] PrimaryKeyFields { get; }

		public DataModel(string name, Model model, DataField[] fields,
			IResourceLoader[] resourceLoaders, DataDomain domain)
		{
			Name = name;
			Model = model;
			Fields = fields;
			ResourceLoaders = resourceLoaders;
			Tables = Fields.Select(q => q.Storage.Table).GroupBy(q => q)
				.Select(q => q.First()).ToArray();
			Domain = domain;
			PrimaryKeyFields = Fields.Where(q => q.Storage.IsPrimaryKey).ToArray();
		}
	}

	public class DataModel<TSource> : DataModel, IView<DataField, TSource>
		where TSource : new()
	{
		public new TypedModel<TSource> Model { get; }

		public DataModel(string name, TypedModel<TSource> model, DataField[] fields,
			IResourceLoader[] resourceLoaders, DataDomain domain)
			: base(name, model, fields, resourceLoaders, domain)
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

			var modelReaders = sources.Select(source => new ObjectReadWriter(typeof(TSource), Model, source)).ToArray();
			var viewContainers = sources.Select(source => new InsertContainer(Model, this)).ToArray();

			GenerateIds(modelReaders);

			this.MapToViewAsync(modelReaders, viewContainers)
				.ConfigureAwait(false).GetAwaiter().GetResult();

			var autoIncField = Fields.FirstOrDefault(q => q.Storage.IsAutoIncrement);

			if (autoIncField == null)
			{
				queries.Add(BulkInsertExpression(columns, table, viewContainers));
				dataProvider.ExecuteNonQuery(QueryExpression.Transaction(queries));
				return;
			}

			foreach (var viewContainer in viewContainers)
			{
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
						if (autoIncField.DataType == typeof(short))
							ids.Add(queryResult.GetInt16(0));
						else if (autoIncField.DataType == typeof(long))
							ids.Add(queryResult.GetInt64(0));
						else
							ids.Add(queryResult.GetInt32(0));
					}
				}
			}
			AssignIds(modelReaders, ids, autoIncField);
		}

		public async Task InsertAsync(IDataProvider dataProvider, params TSource[] sources)
		{
			//  todo: update this to work with datamodels that span multiple tables
			var table = Fields.First().Storage.Table;
			var queries = new List<QueryExpression>();
			var columns = Fields.Where(
				dataField => !dataField.Storage.IsAutoIncrement
				).ToArray();

			var modelReaders = sources.Select(source => new ObjectReadWriter(typeof(TSource), Model, source)).ToArray();
			var viewContainers = sources.Select(source => new InsertContainer(Model, this)).ToArray();

			GenerateIds(modelReaders);

			await this.MapToViewAsync(modelReaders, viewContainers)
				.ConfigureAwait(false);

			var autoIncField = Fields.FirstOrDefault(q => q.Storage.IsAutoIncrement);

			if (autoIncField == null)
			{
				queries.Add(BulkInsertExpression(columns, table, viewContainers));
				await dataProvider.ExecuteNonQueryAsync(QueryExpression.Transaction(queries))
					.ConfigureAwait(false);
				return;
			}

			foreach (var viewContainer in viewContainers)
			{
				var expressionTuple = InsertAndGetIdExpression(columns, table, viewContainer);
				queries.Add(expressionTuple.insert);
				queries.Add(expressionTuple.getId);
			}
			var ids = new List<object>();
			using (var queryResult = await dataProvider.ExecuteReaderAsync(QueryExpression.Transaction(queries))
				.ConfigureAwait(false))
			{
				while (await queryResult.NextResultAsync()
					.ConfigureAwait(false))
				{
					if (await queryResult.ReadAsync()
						.ConfigureAwait(false))
					{
						if (autoIncField.DataType == typeof(short))
							ids.Add(queryResult.GetInt16(0));
						else if (autoIncField.DataType == typeof(long))
							ids.Add(queryResult.GetInt64(0));
						else
							ids.Add(queryResult.GetInt32(0));
					}
				}
			}
			AssignIds(modelReaders, ids, autoIncField);
		}

		public void Update(IDataProvider dataProvider, params TSource[] sources)
		{
			if (PrimaryKeyFields == null ||
				PrimaryKeyFields.Length == 0)
				throw new InvalidOperationException("A primary key is required.");

			//  todo: update this to work with datamodels that span multiple tables
			var table = Fields.First().Storage.Table;
			var tableExpression = QueryExpression.Table(table.TableName);
			var queries = new List<QueryExpression>();
			var columns = Fields.Where(
				dataField => !dataField.Storage.IsPrimaryKey
				).ToArray();

			var modelReaders = sources.Select(source => new ObjectReadWriter(typeof(TSource), Model, source)).ToArray();
			var viewContainers = sources.Select(source => new UpdateContainer(Model, this)).ToArray();

			this.MapToViewAsync(modelReaders, viewContainers)
				.ConfigureAwait(false).GetAwaiter().GetResult();

			foreach (var view in viewContainers)
			{
				queries.Add(QueryExpression.Update(
					tableExpression,
					BuildPrimaryKeyWhereClause(view),
					columns.Select(q => view.AssignExpressions[q.Name]).ToArray()
					));
			}

			dataProvider.ExecuteNonQuery(QueryExpression.Transaction(queries));
		}

		public async Task UpdateAsync(IDataProvider dataProvider, params TSource[] sources)
		{
			if (PrimaryKeyFields == null ||
				PrimaryKeyFields.Length == 0)
				throw new InvalidOperationException("A primary key is required.");

			//  todo: update this to work with datamodels that span multiple tables
			var table = Fields.First().Storage.Table;
			var tableExpression = QueryExpression.Table(table.TableName);
			var queries = new List<QueryExpression>();
			var columns = Fields.Where(
				dataField => !dataField.Storage.IsPrimaryKey
				).ToArray();

			var modelReaders = sources.Select(source => new ObjectReadWriter(typeof(TSource), Model, source)).ToArray();
			var viewContainers = sources.Select(source => new UpdateContainer(Model, this)).ToArray();

			await this.MapToViewAsync(modelReaders, viewContainers)
				.ConfigureAwait(false);

			foreach (var view in viewContainers)
			{
				queries.Add(QueryExpression.Update(
					tableExpression,
					BuildPrimaryKeyWhereClause(view),
					columns.Select(q => view.AssignExpressions[q.Name]).ToArray()
					));
			}

			await dataProvider.ExecuteNonQueryAsync(QueryExpression.Transaction(queries))
				.ConfigureAwait(false);
		}

		public void Delete(IDataProvider dataProvider, params TSource[] sources)
		{
			if (PrimaryKeyFields == null ||
				PrimaryKeyFields.Length == 0)
				throw new InvalidOperationException("A primary key is required.");

			//  todo: update this to work with datamodels that span multiple tables
			var table = Fields.First().Storage.Table;
			var tableExpression = QueryExpression.Table(table.TableName);
			var queries = new List<QueryExpression>();
			var columns = Fields.Where(
				dataField => !dataField.Storage.IsPrimaryKey
				).ToArray();

			var modelReaders = sources.Select(source => new ObjectReadWriter(typeof(TSource), Model, source)).ToArray();
			var viewContainers = sources.Select(source => new UpdateContainer(Model, this)).ToArray();

			//  todo: consider reading primary key fields directly from TSource instead of mapping
			this.MapToViewAsync(modelReaders, viewContainers)
				.ConfigureAwait(false).GetAwaiter().GetResult();

			foreach (var view in viewContainers)
			{
				queries.Add(QueryExpression.Delete(
					tableExpression,
					BuildPrimaryKeyWhereClause(view)
					));
			}

			dataProvider.ExecuteNonQuery(QueryExpression.Transaction(queries));
		}

		public async Task DeleteAsync(IDataProvider dataProvider, params TSource[] sources)
		{
			if (PrimaryKeyFields == null ||
				PrimaryKeyFields.Length == 0)
				throw new InvalidOperationException("A primary key is required.");

			//  todo: update this to work with datamodels that span multiple tables
			var table = Fields.First().Storage.Table;
			var tableExpression = QueryExpression.Table(table.TableName);
			var queries = new List<QueryExpression>();
			var columns = Fields.Where(
				dataField => !dataField.Storage.IsPrimaryKey
				).ToArray();

			var modelReaders = sources.Select(source => new ObjectReadWriter(typeof(TSource), Model, source)).ToArray();
			var viewContainers = sources.Select(source => new UpdateContainer(Model, this)).ToArray();

			//  todo: consider reading primary key fields directly from TSource instead of mapping
			await this.MapToViewAsync(modelReaders, viewContainers)
				.ConfigureAwait(false);

			foreach (var view in viewContainers)
			{
				queries.Add(QueryExpression.Delete(
					tableExpression,
					BuildPrimaryKeyWhereClause(view)
					));
			}

			await dataProvider.ExecuteNonQueryAsync(QueryExpression.Transaction(queries))
				.ConfigureAwait(false);
		}

		private QueryExpression BuildPrimaryKeyWhereClause(IContainer container)
		{
			if (PrimaryKeyFields == null ||
				PrimaryKeyFields.Length == 0)
				throw new InvalidOperationException("A primary key is required.");

			QueryExpression where = null;
			foreach (var field in PrimaryKeyFields)
			{
				var comparison = QueryExpression.Compare(
						QueryExpression.Column(field.Storage.ColumnName),
						ComparisonOperator.AreEqual,
						QueryExpression.Value(
							container.GetValue(field.ModelBinding.ViewFieldPath)
							));
				if (where == null)
				{
					where = comparison;
				}
				else
				{
					where = QueryExpression.AndAlso(where, comparison);
				}
			}
			return where;
		}

		private (QueryExpression insert, QueryExpression getId) InsertAndGetIdExpression(DataField[] columns,
			TableSchema table, InsertContainer viewContainer)
		{
			var row = new QueryExpression[columns.Length];
			for (var i = 0; i < columns.Length; i++)
			{
				row[i] = viewContainer.ColumnValues[columns[i].Storage.ColumnName];
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
					row[i] = container.ColumnValues[columns[i].Storage.ColumnName];
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

		private void AssignIds(IModelReadWriter[] modelReaders, List<object> ids, DataField autoIncField)
		{
			var autoIncModelField = Model.GetField(autoIncField.ModelBinding.ModelFieldPath);
			var modelWriter = new ObjectReadWriter(typeof(TSource), Model, null);

			for (var i = 0; i < modelReaders.Length; i++)
			{
				modelReaders[i].GetField(autoIncModelField).Value = ids[i];
			}
		}

		private void GenerateIds(IModelReadWriter[] modelReaders)
		{
			var autoGenerateField = Fields.FirstOrDefault(q => q.Storage.IsAutoGenerate &&
				q.DataType == typeof(Guid));

			if (autoGenerateField != null)
			{
				var autoGenerateModelField = Model.GetField(autoGenerateField.ModelBinding.ModelFieldPath);

				foreach (var modelReader in modelReaders)
				{
					modelReader.GetField(autoGenerateModelField).Value = Guid.NewGuid();
				}
			}
		}
	}

	public class DataModel<TSource, TView> : DataModel<TSource>, IView<DataField, TSource, TView>
		where TSource : new()
		where TView: new()
	{
		public DataModel(string name, TypedModel<TSource> model, DataField[] fields,
			IResourceLoader[] resourceLoaders, DataDomain domain)
			: base(name, model, fields, resourceLoaders, domain)
		{
		}
	}
}
