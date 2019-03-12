using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Mapping;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class EntityModelHelper<T>
		where T : class
	{
		public PropertyInfoField From { get; }
		public EntityField To { get; }
		public IFieldPath<EntityModel, EntityField> ToPath { get; }
		public string ObjectPath { get; }

		public EntityModelHelper(PropertyInfoField from, EntityField to,
			IFieldPath<EntityModel, EntityField> toPath, string objectPath)
		{
			From = from;
			To = to;
			ToPath = toPath;
			ObjectPath = objectPath;
		}

		public ValueExpression WriteValueExpression(T from)
		{
			var reader = new ObjectGraphReaderWriter<T>(from);
			return WriteValueExpression(reader);
		}

		public abstract ValueExpression WriteValueExpression(IGraphReader<TypeModel, PropertyInfoField> from);
	}

	public class EntityModelHelper<TEntity, TFromValue, TToValue>
		: EntityModelHelper<TEntity>
		where TEntity : class
	{
		private readonly TryConvertDelegate<TFromValue, TToValue> _converter;
		private readonly IFieldPath<TypeModel, PropertyInfoField> _fromPath;

		public EntityModelHelper(
			IntersectedFields<TypeModel, PropertyInfoField, EntityModel, EntityField, TFromValue, TToValue> intersectedFields,
			string objectPath
			)
			: base(intersectedFields.LeftField, intersectedFields.RightField, intersectedFields.RightPath, objectPath)
		{
			_converter = intersectedFields.GetConvertDelegate();
			_fromPath = intersectedFields.LeftPath;
		}

		public override ValueExpression WriteValueExpression(IGraphReader<TypeModel, PropertyInfoField> from)
		{
			if (!from.CheckPath(_fromPath))
				return null;

			var sourceValue = from.Read<TFromValue>(_fromPath);
			if (_converter(sourceValue, out var destValue))
				return ORMQueryExpressions.Value(destValue);
			return null;
		}
	}
}
