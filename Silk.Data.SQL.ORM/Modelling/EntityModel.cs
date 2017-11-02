using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
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
	public abstract class EntityModel : IView<DataField>
	{
		private Lazy<DataDomain> _domain;

		public abstract Type EntityType { get; }

		public DataField[] Fields { get; private set; }

		public string Name { get; private set; }

		public Model Model { get; protected set; }

		public IResourceLoader[] ResourceLoaders { get; private set; }

		IViewField[] IView.Fields => Fields;

		public EntitySchema Schema { get; private set; }

		public DataDomain Domain => _domain.Value;

		public DataField[] PrimaryKeyFields { get; private set; }

		protected EntityModel() { }

		public EntityModel(string name, Model model,
			EntitySchema schema, DataField[] fields,
			Lazy<DataDomain> domain)
		{
			Name = name;
			Model = model;
			Fields = fields;
			ResourceLoaders = fields
				.Where(q => q.ModelBinding.ResourceLoaders != null)
				.SelectMany(q => q.ModelBinding.ResourceLoaders)
				.GroupBy(q => q)
				.Select(q => q.First())
				.ToArray();
			Schema = schema;
			PrimaryKeyFields = Fields.Where(q => q.Storage.IsPrimaryKey).ToArray();
			_domain = domain;
		}

		internal void Initalize(string name, Model model,
			EntitySchema schema, IEnumerable<DataField> fields,
			Lazy<DataDomain> domain)
		{
			Name = name;
			Model = model;
			Fields = fields.ToArray();
			ResourceLoaders = fields
				.Where(q => q.ModelBinding.ResourceLoaders != null)
				.SelectMany(q => q.ModelBinding.ResourceLoaders)
				.GroupBy(q => q)
				.Select(q => q.First())
				.ToArray();
			Schema = schema;
			_domain = domain;
			PrimaryKeyFields = Fields.Where(q => q.Storage.IsPrimaryKey).ToArray();
		}

		public abstract void SetModel(Model model);
	}

	public class EntityModel<TSource> : EntityModel, IView<DataField, TSource>
		where TSource : new()
	{
		private readonly Dictionary<Type, EntityModel<TSource>> _cachedSubViews
			= new Dictionary<Type, EntityModel<TSource>>();

		public new TypedModel<TSource> Model { get; private set; }

		public override Type EntityType => typeof(TSource);

		internal EntityModel() { }

		public EntityModel(string name, TypedModel<TSource> model,
			EntitySchema schema, DataField[] fields,
			Lazy<DataDomain> domain)
			: base(name, model, schema, fields, domain)
		{
			Model = model;
		}

		public override void SetModel(Model model)
		{
			Model = model as TypedModel<TSource>;
			base.Model = model;
		}

		public EntityModel<TSource, TView> GetSubView<TView>()
			where TView : new()
		{
			return Domain.GetProjectionModel<TSource, TView>();

			//if (_cachedSubViews.TryGetValue(typeof(TView), out var subView))
			//{
			//	return subView as EntityModel<TSource, TView>;
			//}

			//lock (_cachedSubViews)
			//{
			//	if (_cachedSubViews.TryGetValue(typeof(TView), out subView))
			//	{
			//		return subView as EntityModel<TSource, TView>;
			//	}

			//	var compareDataModel = Domain.CreateDataModel<TSource, TView>();
			//	var subViewFields = new List<DataField>();

			//	foreach (var compareField in compareDataModel.Fields)
			//	{
			//		var matchedField = Fields.FirstOrDefault(
			//			realField => realField.ModelBinding.ModelFieldPath.SequenceEqual(compareField.ModelBinding.ModelFieldPath) &&
			//				realField.ModelBinding.ViewFieldPath.SequenceEqual(compareField.ModelBinding.ViewFieldPath)
			//			);
			//		if (matchedField == null)
			//			continue;

			//		subViewFields.Add(new DataField(
			//			compareField.Storage.ColumnName, compareField.DataType, compareField.Metadata,
			//			compareField.ModelBinding, matchedField.Storage.Table, compareField.Relationship, compareField.Name
			//			));
			//	}

			//	var resourceLoaders = subViewFields
			//		.Where(q => q.ModelBinding.ResourceLoaders != null)
			//		.SelectMany(q => q.ModelBinding.ResourceLoaders)
			//		.GroupBy(q => q)
			//		.Select(q => q.First())
			//		.ToArray();
			//	var ret = new EntityModel<TSource, TView>(typeof(TView).Name, Model, null, subViewFields.ToArray(), resourceLoaders,
			//		Domain);
			//	_cachedSubViews.Add(typeof(TView), ret);
			//	return ret;
			//}
		}

		public void MapToView(ICollection<IModelReadWriter> modelReadWriters, ICollection<IContainer> viewContainers)
		{
			using (var readWriterEnum = modelReadWriters.GetEnumerator())
			using (var containerEnum = viewContainers.GetEnumerator())
			{
				var mappingContext = new MappingContext(BindingDirection.ModelToView);

				while (readWriterEnum.MoveNext() &&
					containerEnum.MoveNext())
				{
					var modelReadWriter = readWriterEnum.Current;
					var viewContainer = containerEnum.Current;
					foreach (var viewField in Fields)
					{
						if ((viewField.ModelBinding.Direction & BindingDirection.ModelToView) == BindingDirection.ModelToView)
						{
							var value = viewField.ModelBinding.ReadFromModel(modelReadWriter, mappingContext);
							if (value != null)
							{
								viewField.ModelBinding.WriteToContainer(viewContainer,
									value,
									mappingContext);
							}
						}
					}
				}
			}
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

	public class EntityModel<TSource, TView> : EntityModel<TSource>, IView<DataField, TSource, TView>
		where TSource : new()
		where TView: new()
	{
		internal EntityModel() { }

		public EntityModel(string name, TypedModel<TSource> model,
			EntitySchema schema, DataField[] fields,
			Lazy<DataDomain> domain)
			: base(name, model, schema, fields, domain)
		{
		}

		public ConditionExpression<TSource, TView> Where(Expression<Func<TView, bool>> expression)
		{
			return new ConditionExpression<TSource, TView>(this, expression);
		}
	}
}
