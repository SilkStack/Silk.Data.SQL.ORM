﻿using Silk.Data.Modelling;
using Silk.Data.Modelling.ResourceLoaders;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
		private static readonly TSource[] _noResults = new TSource[0];

		public new TypedModel<TSource> Model { get; }

		public DataModel(string name, TypedModel<TSource> model, DataField[] fields,
			IResourceLoader[] resourceLoaders, DataDomain domain)
			: base(name, model, fields, resourceLoaders, domain)
		{
			Model = model;
		}

		public void MapToView(ICollection<IModelReadWriter> modelReadWriters, ICollection<IContainer> viewContainers)
		{
			//  todo: replace this with a non-async map method built for datamodels specifically
			//		this will NOT support loading resources when mapping TO views
			this.MapToViewAsync(modelReadWriters, viewContainers)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}

		public IReadOnlyCollection<TSource> Select(IDataProvider dataProvider,
			QueryExpression where = null,
			int? offset = null,
			int? limit = null)
		{
			//  todo: update this to work with datamodels that span multiple tables
			var table = Fields.First().Storage.Table;
			var results = new List<TSource>();
			var resultWriters = new List<IModelReadWriter>();
			var rows = new List<IContainer>();

			if (where is Expressions.ConditionExpression conditionExpr)
				where = conditionExpr.Expression;

			using (var queryResult = dataProvider.ExecuteReader(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table(table.TableName),
					where: where,
					offset: offset != null ? QueryExpression.Value(offset.Value) : null,
					limit: limit != null ? QueryExpression.Value(limit.Value) : null
				)))
			{
				if (!queryResult.HasRows)
					return _noResults;

				while (queryResult.Read())
				{
					var result = new TSource();
					var container = new RowContainer(Model, this);
					container.ReadRow(queryResult);
					rows.Add(container);
					resultWriters.Add(new ObjectReadWriter(typeof(TSource), Model, result));
					results.Add(result);
				}
			}

			this.MapToModelAsync(resultWriters, rows)
				.ConfigureAwait(false)
				.GetAwaiter().GetResult();

			return results;
		}

		public async Task<IReadOnlyCollection<TSource>> SelectAsync(IDataProvider dataProvider,
			QueryExpression where = null,
			int? offset = null,
			int? limit = null)
		{
			//  todo: update this to work with datamodels that span multiple tables
			var table = Fields.First().Storage.Table;
			var results = new List<TSource>();
			var resultWriters = new List<IModelReadWriter>();
			var rows = new List<IContainer>();

			if (where is Expressions.ConditionExpression conditionExpr)
				where = conditionExpr.Expression;

			using (var queryResult = await dataProvider.ExecuteReaderAsync(
				QueryExpression.Select(
					new[] { QueryExpression.All() },
					from: QueryExpression.Table(table.TableName),
					where: where,
					offset: offset != null ? QueryExpression.Value(offset.Value) : null,
					limit: limit != null ? QueryExpression.Value(limit.Value) : null
				)).ConfigureAwait(false))
			{
				if (!queryResult.HasRows)
					return _noResults;

				while (await queryResult.ReadAsync()
					.ConfigureAwait(false))
				{
					var result = new TSource();
					var container = new RowContainer(Model, this);
					container.ReadRow(queryResult);
					rows.Add(container);
					resultWriters.Add(new ObjectReadWriter(typeof(TSource), Model, result));
					results.Add(result);
				}
			}

			await this.MapToModelAsync(resultWriters, rows)
				.ConfigureAwait(false);

			return results;
		}

		public ModelBoundExecutableQueryCollection<TSource> Insert(params TSource[] sources)
		{
			return new ModelBoundExecutableQueryCollection<TSource>(this)
				.Insert(sources);
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
					columns.Select(q =>
					{
						if (view.AssignExpressions.TryGetValue(q.Name, out var val))
							return val;
						return QueryExpression.Assign(QueryExpression.Column(q.Storage.ColumnName), QueryExpression.Value(null));
					}).ToArray()
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
					columns.Select(q =>
					{
						if (view.AssignExpressions.TryGetValue(q.Name, out var val))
							return val;
						return QueryExpression.Assign(QueryExpression.Column(q.Storage.ColumnName), QueryExpression.Value(null));
					}).ToArray()
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

		public ConditionExpression<TSource, TView> Where(Expression<Func<TView, bool>> expression)
		{
			return new ConditionExpression<TSource, TView>(this, expression);
		}
	}
}
