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
		public abstract Type EntityType { get; }

		public DataField[] Fields { get; internal set; }

		public string Name { get; internal set; }

		public Model Model { get; protected set; }

		public IResourceLoader[] ResourceLoaders { get; internal set; }

		IViewField[] IView.Fields => Fields;

		public EntitySchema Schema { get; internal set; }

		public DataDomain Domain { get; internal set; }

		public DataField[] PrimaryKeyFields { get; internal set; }

		protected EntityModel() { }

		public EntityModel(string name, Model model,
			EntitySchema schema, DataField[] fields,
			DataDomain domain)
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
			Domain = domain;
		}

		internal virtual void SetResourceLoaders()
		{
			ResourceLoaders = Fields
				.Where(q => q.ModelBinding.ResourceLoaders != null)
				.SelectMany(q => q.ModelBinding.ResourceLoaders)
				.GroupBy(q => q)
				.Select(q => q.First())
				.ToArray();
		}

		internal virtual void Initalize(string name, Model model,
			EntitySchema schema, IEnumerable<DataField> fields,
			DataDomain domain)
		{
			Name = name;
			Model = model;
			Fields = fields.ToArray();
			ResourceLoaders = fields
				.Where(q => q.ModelBinding?.ResourceLoaders != null)
				.SelectMany(q => q.ModelBinding.ResourceLoaders)
				.GroupBy(q => q)
				.Select(q => q.First())
				.ToArray();
			Schema = schema;
			Domain = domain;
			PrimaryKeyFields = Fields.Where(q => q.Storage?.IsPrimaryKey == true).ToArray();
		}

		internal void AddField(DataField field)
		{
			//  todo: I know this is super ineffecient but the IView<T> interface specifies an array property for Fields
			//  investigate fixing that!
			var copy = Fields;
			Fields = new DataField[Fields.Length + 1];
			Array.Copy(copy, Fields, copy.Length);
			Fields[copy.Length] = field;
		}

		public abstract void SetModel(Model model);

		public abstract Model GetAsModel();
	}

	public class NonQueryableEntityModel<TSource> : EntityModel, IView<DataField, TSource>
	{
		public override Type EntityType => typeof(TSource);

		public new TypedModel<TSource> Model { get; private set; }

		internal NonQueryableEntityModel() { }

		internal override void Initalize(string name, Model model, EntitySchema schema, IEnumerable<DataField> fields, DataDomain domain)
		{
			base.Initalize(name, model, schema, fields, domain);
			Model = model as TypedModel<TSource>;
		}

		public override Model GetAsModel()
		{
			var fields = new List<ModelField>();
			foreach (var field in Fields)
			{
				var direction = field.ModelBinding.Direction;
				var canRead = direction.HasFlag(BindingDirection.ModelToView);
				var canWrite = direction.HasFlag(BindingDirection.ViewToModel);
				var dataType = field.DataType;
				Type enumType = null;
				if (field.Relationship?.RelationshipType == RelationshipType.ManyToMany)
					enumType = typeof(IEnumerable<>).MakeGenericType(field.DataType);
				fields.Add(new ModelField(
					field.Name, canRead, canWrite,
					field.Metadata.Concat(new object[] { field.Storage }).ToArray(),
					dataType, enumType
					));
			}
			return new Model(Name, fields, Model.Metadata);
		}

		public override void SetModel(Model model)
		{
			Model = model as TypedModel<TSource>;
			base.Model = model;
		}
	}

	public class EntityModel<TSource> : EntityModel, IView<DataField, TSource>
		where TSource : new()
	{
		public new TypedModel<TSource> Model { get; private set; }

		public override Type EntityType => typeof(TSource);

		internal EntityModel() { }

		internal override void Initalize(string name, Model model, EntitySchema schema, IEnumerable<DataField> fields, DataDomain domain)
		{
			base.Initalize(name, model, schema, fields, domain);
			Model = model as TypedModel<TSource>;
		}

		public override void SetModel(Model model)
		{
			Model = model as TypedModel<TSource>;
			base.Model = model;
		}

		public override Model GetAsModel()
		{
			var fields = new List<ModelField>();
			foreach (var field in Fields)
			{
				var direction = field.ModelBinding.Direction;
				var canRead = direction.HasFlag(BindingDirection.ModelToView);
				var canWrite = direction.HasFlag(BindingDirection.ViewToModel);
				var dataType = field.DataType;
				Type enumType = null;
				if (field.Relationship?.RelationshipType == RelationshipType.ManyToMany)
					enumType = typeof(IEnumerable<>).MakeGenericType(field.DataType);
				fields.Add(new ModelField(
					field.Name, canRead, canWrite,
					field.Metadata.Concat(new object[] { field.Storage }).ToArray(),
					dataType, enumType
					));
			}
			return new Model(Name, fields, Model.Metadata);
		}

		public void MapToView(ICollection<ModelReadWriter> modelReadWriters, ICollection<ViewReadWriter> viewReadWriters)
		{
			using (var readWriterEnum = modelReadWriters.GetEnumerator())
			using (var containerEnum = viewReadWriters.GetEnumerator())
			{
				var mappingContext = new MappingContext(BindingDirection.ModelToView);

				while (readWriterEnum.MoveNext() &&
					containerEnum.MoveNext())
				{
					var modelReadWriter = readWriterEnum.Current;
					var viewReadWriter = containerEnum.Current;
					foreach (var viewField in Fields)
					{
						if ((viewField.ModelBinding.Direction & BindingDirection.ModelToView) == BindingDirection.ModelToView)
						{
							viewField.ModelBinding
								.CopyBindingValue(modelReadWriter, viewReadWriter, mappingContext);
						}
					}
				}
			}
		}

		public IEnumerable<ORMQuery> Select(
			QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
		{
			return new SelectQueryBuilder<TSource>(this)
				.CreateQuery(where, having, orderBy, groupBy, offset, limit);
		}

		public IEnumerable<ORMQuery> Select<TView>(
			QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] orderBy = null,
			QueryExpression[] groupBy = null,
			int? offset = null,
			int? limit = null)
			where TView : new()
		{
			return new SelectQueryBuilder<TSource>(this)
				.CreateQuery<TView>(where, having, orderBy, groupBy, offset, limit);
		}

		public IEnumerable<ORMQuery> SelectCount(
			QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] groupBy = null)
		{
			return new SelectQueryBuilder<TSource>(this)
				.CreateCountQuery(where, having, groupBy);
		}

		public IEnumerable<ORMQuery> SelectCount<TView>(
			QueryExpression where = null,
			QueryExpression having = null,
			QueryExpression[] groupBy = null)
			where TView : new()
		{
			return new SelectQueryBuilder<TSource>(this)
				.CreateCountQuery<TView>(where, having, groupBy);
		}

		public IEnumerable<ORMQuery> Insert(params TSource[] sources)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ORMQuery> Insert(IEnumerable<TSource> sources)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ORMQuery> Insert<TView>(params TView[] sources)
			where TView : new()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ORMQuery> Insert<TView>(IEnumerable<TView> sources)
			where TView : new()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ORMQuery> Update(params TSource[] sources)
		{
			return new UpdateQueryBuilder<TSource>(this)
				.CreateQuery(sources);
		}

		public IEnumerable<ORMQuery> Update(IEnumerable<TSource> sources)
		{
			return new UpdateQueryBuilder<TSource>(this)
				.CreateQuery(sources);
		}

		public IEnumerable<ORMQuery> Update<TView>(params TView[] sources)
			where TView : new()
		{
			return new UpdateQueryBuilder<TSource>(this)
				.CreateQuery(sources);
		}

		public IEnumerable<ORMQuery> Update<TView>(IEnumerable<TView> sources)
			where TView : new()
		{
			return new UpdateQueryBuilder<TSource>(this)
				.CreateQuery(sources);
		}

		public IEnumerable<ORMQuery> Delete(params TSource[] sources)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ORMQuery> Delete(IEnumerable<TSource> sources)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ORMQuery> Delete<TView>(params TView[] sources)
			where TView : new()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ORMQuery> Delete<TView>(IEnumerable<TView> sources)
			where TView : new()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ORMQuery> Delete(QueryExpression where = null)
		{
			throw new NotImplementedException();
		}

		public ConditionExpression<TSource> Where(Expression<Func<TSource, bool>> expression)
		{
			return new ConditionExpression<TSource>(this, expression);
		}
	}

	public class EntityModel<TSource, TView> : EntityModel<TSource>, IView<DataField, TSource, TView>
		where TSource : new()
		where TView: new()
	{
		internal EntityModel() { }

		public ConditionExpression<TSource, TView> Where(Expression<Func<TView, bool>> expression)
		{
			return new ConditionExpression<TSource, TView>(this, expression);
		}
	}
}
