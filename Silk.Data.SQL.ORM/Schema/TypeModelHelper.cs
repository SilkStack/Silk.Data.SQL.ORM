using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Mapping;
using Silk.Data.SQL.ORM.Queries;
using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class TypeModelHelper<T>
		where T : class
	{
		public EntityField From { get; }
		public PropertyInfoField To { get; }

		public abstract IFieldPath<TypeModel, PropertyInfoField> ToPath { get; }

		public TypeModelHelper(EntityField from, PropertyInfoField to)
		{
			From = from;
			To = to;
		}

		public void WriteValueToInstance<TValue>(T instance, TValue value)
		{
			var writer = new ObjectGraphReaderWriter<T>(instance);
			WriteValueToInstance(writer, value);
		}

		public void CopyResultToObject(QueryResult result, T to)
		{
			var writer = new ObjectGraphReaderWriter<T>(to);
			var reader = new QueryGraphReader(result);
			CopyResultToObject(reader, writer);
		}

		public abstract void WriteValueToInstance<TValue>(ObjectGraphReaderWriter<T> graphWriter, TValue value);
		public abstract void CopyResultToObject(QueryGraphReader reader, ObjectGraphReaderWriter<T> writer);
	}

	public class TypeModelHelper<TEntity, TFromValue, TToValue> : TypeModelHelper<TEntity>
		where TEntity : class
	{
		private readonly TryConvertDelegate<TFromValue, TToValue> _converter;
		private readonly IFieldPath<EntityModel, EntityField> _fromPath;
		private readonly IFieldPath<TypeModel, PropertyInfoField> _toPath;

		public override IFieldPath<TypeModel, PropertyInfoField> ToPath => _toPath;

		public TypeModelHelper(
			IntersectedFields<EntityModel, EntityField, TypeModel, PropertyInfoField, TFromValue, TToValue> intersectedFields
			)
			: base(intersectedFields.LeftField, intersectedFields.RightField)
		{
			_converter = intersectedFields.GetConvertDelegate();
			_fromPath = intersectedFields.LeftPath;
			_toPath = intersectedFields.RightPath;
		}

		public override void WriteValueToInstance<TValue>(ObjectGraphReaderWriter<TEntity> graphWriter, TValue value)
		{
			graphWriter.Write<TValue>(_toPath, value);
		}

		public override void CopyResultToObject(QueryGraphReader reader, ObjectGraphReaderWriter<TEntity> writer)
		{
			if (!reader.CheckPath(_fromPath))
				return;
			var value = reader.Read<TFromValue>(_fromPath);
			if (_converter(value, out var newValue))
				writer.Write(_toPath, newValue);
		}
	}
}
