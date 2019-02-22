using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Mapping;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Queries;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class IntersectedFieldWriter<T>
		where T : class
	{
		public PropertyInfoField From { get; }
		public EntityField To { get; }

		public IntersectedFieldWriter(PropertyInfoField from, EntityField to)
		{
			From = from;
			To = to;
		}

		public void Write(T from, IFieldAssignmentBuilder fieldAssignmentBuilder)
		{
			var reader = new ObjectGraphReaderWriter<T>(from);
			Write(reader, fieldAssignmentBuilder);
		}

		public abstract void Write(ObjectGraphReaderWriter<T> from, IFieldAssignmentBuilder fieldAssignmentBuilder);
	}

	public class IntersectedFieldWriter<TEntity, TFromValue, TToValue>
		: IntersectedFieldWriter<TEntity>
		where TEntity : class
	{
		private readonly TryConvertDelegate<TFromValue, TToValue> _converter;
		private readonly IFieldPath<TypeModel, PropertyInfoField> _fromPath;

		public IntersectedFieldWriter(
			IntersectedFields<TypeModel, PropertyInfoField, EntityModel, EntityField, TFromValue, TToValue> intersectedFields
			)
			: base(intersectedFields.LeftField, intersectedFields.RightField)
		{
			_converter = intersectedFields.GetConvertDelegate();
			_fromPath = intersectedFields.LeftPath;
		}

		public override void Write(ObjectGraphReaderWriter<TEntity> from, IFieldAssignmentBuilder fieldAssignmentBuilder)
		{
			var sourceValue = from.Read<TFromValue>(_fromPath);
			if (_converter(sourceValue, out var destValue))
				fieldAssignmentBuilder.Set(QueryExpression.Column(To.Column.Name), ORMQueryExpressions.Value(destValue));
		}
	}
}
