using Silk.Data.Modelling;
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

		public ModelBoundExecutableQueryCollection<TSource> Update(params TSource[] sources)
		{
			return new ModelBoundExecutableQueryCollection<TSource>(this)
				.Update(sources);
		}

		public ModelBoundExecutableQueryCollection<TSource> Delete(params TSource[] sources)
		{
			return new ModelBoundExecutableQueryCollection<TSource>(this)
				.Delete(sources);
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
