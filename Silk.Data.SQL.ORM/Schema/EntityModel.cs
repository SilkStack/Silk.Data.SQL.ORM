using System;
using System.Collections.Generic;
using System.Linq;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.GenericDispatch;

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
			Table = new Table(tableName, Fields.Where(q => q.Column != null).Select(q => q.Column));
			Indexes = indexes?.ToArray() ?? new Index[0];
		}

		public abstract void Dispatch(IModelGenericExecutor executor);

		public abstract IModelTranscriber<TView> GetModelTranscriber<TView>(
			TypeModel<TView> typeModel = null
			)
			where TView : class;

		public IEnumerable<EntityField> GetPathFields(IFieldPath<EntityField> fieldPath)
			=> fieldPath.FinalField.SubFields;
	}

	/// <summary>
	/// Model of an entity type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EntityModel<T> : EntityModel
		where T : class
	{
		private static readonly TypeModel<T> _typeModel = Modelling.TypeModel.GetModelOf<T>();

		public new TypeModel<T> TypeModel => _typeModel;

		public override Type EntityType => typeof(T);

		public new IReadOnlyList<EntityField<T>> Fields { get; }

		private readonly Dictionary<Type, IModelTranscriber> _transcriberCache
			= new Dictionary<Type, IModelTranscriber>();

		public EntityModel(IEnumerable<EntityField<T>> entityFields, string tableName = null,
			IEnumerable<Index> indexes = null) :
			base(entityFields, tableName ?? typeof(T).Name, indexes, _typeModel)
		{
			Fields = entityFields.ToArray();
		}

		public override void Dispatch(IModelGenericExecutor executor)
			=> executor.Execute<EntityModel, EntityField, T>(this);

		private IIntersection<TypeModel, PropertyInfoField, EntityModel, EntityField> GetTypeToModelIntersection<T1>(
			TypeModel<T1> typeModel = null
			)
		{
			if (typeModel == null)
				typeModel = Modelling.TypeModel.GetModelOf<T1>();

			var analyzer = new TypeModelToEntityModelIntersectionAnalyzer();
			return analyzer.CreateIntersection(typeModel, this);
		}

		public override IModelTranscriber<TView> GetModelTranscriber<TView>(
			TypeModel<TView> typeModel = default(TypeModel<TView>)
			)
		{
			var type = typeof(TView);
			if (_transcriberCache.TryGetValue(type, out var transcriber))
				return transcriber as IModelTranscriber<TView>;

			lock (_transcriberCache)
			{
				if (_transcriberCache.TryGetValue(type, out transcriber))
					return transcriber as IModelTranscriber<TView>;

				if (typeModel == null)
					typeModel = Modelling.TypeModel.GetModelOf<TView>();
				var typeToModelIntersection = GetTypeToModelIntersection(typeModel);
				transcriber = ModelTranscriberFactory.Create<T, TView>(typeToModelIntersection);
				_transcriberCache.Add(type, transcriber);

				return transcriber as IModelTranscriber<TView>;
			}
		}
	}
}
