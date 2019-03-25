using System;
using System.Collections.Generic;
using System.Linq;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.GenericDispatch;
using Silk.Data.SQL.ORM.Modelling;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Model of an entity type.
	/// </summary>
	public abstract class EntityModel : IModel<EntityField>
	{
		public Table Table { get; }

		public IReadOnlyList<EntityField> Fields { get; }
		IReadOnlyList<IField> IModel.Fields => Fields;

		public TypeModel TypeModel { get; }

		public abstract Type EntityType { get; }

		public IReadOnlyList<Index> Indexes { get; }

		protected EntityModel(IEnumerable<EntityField> entityFields,
			string tableName, IEnumerable<Index> indexes, TypeModel typeModel)
		{
			Fields = entityFields.ToArray();
			Table = new Table(tableName, GetAllLocalColumns().Where(q => q != null));
			Indexes = indexes?.ToArray() ?? new Index[0];
		}

		private IEnumerable<Column> GetAllLocalColumns()
		{
			foreach (var field in Fields)
			{
				if (field.IsEntityLocalField)
				{
					yield return field.Column;
					foreach (var column in GetSubColumns(field))
						yield return column;
				}
			}

			IEnumerable<Column> GetSubColumns(EntityField field)
			{
				foreach (var subField in field.SubFields)
				{
					if (!subField.IsEntityLocalField)
						continue;

					yield return subField.Column;
					foreach (var column in GetSubColumns(subField))
						yield return column;
				}
			}
		}

		public abstract IEntityView<TView> GetEntityView<TView>(
			TypeModel<TView> typeModel = null
			)
			where TView : class;

		public IEnumerable<EntityField> GetPathFields(IFieldPath<EntityField> fieldPath)
			=> fieldPath.FinalField.SubFields;

		public abstract void Dispatch(IModelGenericExecutor executor);
	}

	/// <summary>
	/// Model of an entity type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EntityModel<T> : EntityModel
		where T : class
	{
		private static readonly TypeModel<T> _typeModel = Silk.Data.Modelling.TypeModel.GetModelOf<T>();

		public new TypeModel<T> TypeModel => _typeModel;

		public override Type EntityType => typeof(T);

		public new IReadOnlyList<EntityField<T>> Fields { get; }

		private readonly ClassToEntityIntersectionAnalyzer _classToEntityAnalyzer;
		private readonly IIntersectionAnalyzer<EntityModel, EntityField, TypeModel, PropertyInfoField> _modelToTypeAnalyzer;
		private readonly Dictionary<Type, IEntityView> _viewCache
			= new Dictionary<Type, IEntityView>();

		public EntityModel(
			ClassToEntityIntersectionAnalyzer classToEntityIntersectionAnalyzer,
			IIntersectionAnalyzer<EntityModel, EntityField, TypeModel, PropertyInfoField> modelToTypeAnalyzer,
			IEnumerable<EntityField<T>> entityFields, string tableName = null,
			IEnumerable<Index> indexes = null
			) :
			base(entityFields, tableName ?? typeof(T).Name, indexes, _typeModel)
		{
			Fields = entityFields.ToArray();
			_classToEntityAnalyzer = classToEntityIntersectionAnalyzer;
			_modelToTypeAnalyzer = modelToTypeAnalyzer;
		}

		public override IEntityView<TView> GetEntityView<TView>(
			TypeModel<TView> typeModel = default(TypeModel<TView>)
			)
		{
			var type = typeof(TView);
			if (_viewCache.TryGetValue(type, out var view))
				return view as IEntityView<TView>;

			lock (_viewCache)
			{
				if (_viewCache.TryGetValue(type, out view))
					return view as IEntityView<TView>;

				if (typeModel == null)
					typeModel = Data.Modelling.TypeModel.GetModelOf<TView>();
				view = EntityViewFactory.Create<T, TView>(
					_classToEntityAnalyzer, _modelToTypeAnalyzer, this
					);
				_viewCache.Add(type, view);

				return view as IEntityView<TView>;
			}
		}

		public override void Dispatch(IModelGenericExecutor executor)
			=> executor.Execute<EntityModel, EntityField>(this);
	}
}
