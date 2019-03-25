using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Mapping;

namespace Silk.Data.SQL.ORM
{
	public abstract class PrimaryKeyEntityReferenceFactory<T>
		where T : class
	{
		public abstract PrimaryKeyEntityReference<T> Create(ITypeInstanceFactory typeInstanceFactory, string primaryKeyValue);
	}

	public class PrimaryKeyEntityReferenceFactory<T, TId> : PrimaryKeyEntityReferenceFactory<T>
		where T : class
	{
		private readonly IFieldPath<TypeModel, PropertyInfoField> _primaryKeyPath;
		private readonly TryConvertDelegate<string, TId> _tryParseId;

		public PrimaryKeyEntityReferenceFactory(IFieldPath<TypeModel, PropertyInfoField> primaryKeyPath,
			TryConvertDelegate<string, TId> tryParseId)
		{
			_primaryKeyPath = primaryKeyPath;
			_tryParseId = tryParseId;
		}

		public override PrimaryKeyEntityReference<T> Create(ITypeInstanceFactory typeInstanceFactory, string primaryKeyValue)
		{
			if (!_tryParseId(primaryKeyValue, out var primaryKeyId))
				return null;

			var blankEntity = typeInstanceFactory.CreateInstance<T>();
			var writer = new ObjectGraphReaderWriter<T>(blankEntity);
			writer.Write(_primaryKeyPath, primaryKeyId);
			return new PrimaryKeyEntityReference<T>(blankEntity);
		}
	}
}
