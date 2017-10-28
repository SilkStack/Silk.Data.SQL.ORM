using Silk.Data.Modelling;
using Silk.Data.Modelling.ResourceLoaders;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Expressions;
using Silk.Data.SQL.ORM.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Silk.Data.SQL.ORM.Modelling
{
	public abstract class DataModel : IView<DataField>
	{
		public abstract Type EntityType { get; }

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
		private readonly Dictionary<Type, DataModel<TSource>> _cachedSubViews
			= new Dictionary<Type, DataModel<TSource>>();

		public new TypedModel<TSource> Model { get; }

		public override Type EntityType => typeof(TSource);

		public DataModel(string name, TypedModel<TSource> model, DataField[] fields,
			IResourceLoader[] resourceLoaders, DataDomain domain)
			: base(name, model, fields, resourceLoaders, domain)
		{
			Model = model;
		}

		public DataModel<TSource, TView> GetSubView<TView>()
			where TView : new()
		{
			if (_cachedSubViews.TryGetValue(typeof(TView), out var subView))
			{
				return subView as DataModel<TSource, TView>;
			}

			lock (_cachedSubViews)
			{
				if (_cachedSubViews.TryGetValue(typeof(TView), out subView))
				{
					return subView as DataModel<TSource, TView>;
				}

				var compareDataModel = Domain.CreateDataModel<TSource, TView>();
				//  scan through the compare model and get all the fields bound on TSource's model
				var modelFields = compareDataModel.Fields
					.Select(compareSourceField => new {
						Binding = compareSourceField.ModelBinding,
						Field = Model.GetField(compareSourceField.ModelBinding.ModelFieldPath)
					})
					.Where(modelField => modelField.Field != null)
					.ToArray();
				//  then get the bound field's binding from THIS model
				var storageFields = Fields.Where(q =>
					modelFields.Select(q2 => q2.Binding.ModelFieldPath).Any(q2 => q2.SequenceEqual(q.ModelBinding.ModelFieldPath)) &&
					modelFields.Select(q2 => q2.Binding.ViewFieldPath).Any(q2 => q2.SequenceEqual(q.ModelBinding.ViewFieldPath)))
					.ToArray();
				//  and project that onto a new DataModel
				var resourceLoaders = storageFields
					.Where(q => q.ModelBinding.ResourceLoaders != null)
					.SelectMany(q => q.ModelBinding.ResourceLoaders)
					.GroupBy(q => q)
					.Select(q => q.First())
					.ToArray();
				var ret = new DataModel<TSource, TView>(nameof(TView), Model, storageFields, resourceLoaders,
					Domain);
				_cachedSubViews.Add(typeof(TView), ret);
				return ret;
			}
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

		public ModelBoundExecutableQueryCollection<TSource, TSource> Select(
			QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
		{
			return new ModelBoundExecutableQueryCollection<TSource>(this)
				.Select(where, having, orderBy, groupBy, offset, limit);
		}

		public ModelBoundExecutableQueryCollection<TSource, TView> Select<TView>(
			QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
			where TView : new()
		{
			return new ModelBoundExecutableQueryCollection<TSource>(this)
				.Select<TView>(where, having, orderBy, groupBy, offset, limit);
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

		public ModelBoundExecutableQueryCollection<TSource> Delete(QueryExpression where = null)
		{
			return new ModelBoundExecutableQueryCollection<TSource>(this)
				.Delete(where: where);
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
