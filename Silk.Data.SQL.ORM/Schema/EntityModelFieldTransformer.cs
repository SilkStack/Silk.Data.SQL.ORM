using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Mapping;
using System.Linq;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class EntityModelFieldTransformer
	{
		public abstract string SourcePath { get; }

		public abstract IGraphReader<TypeModel, PropertyInfoField> ReadTransformed(
			IGraphReader<TypeModel, PropertyInfoField> graphReader
			);
	}

	public abstract class EntityModelFieldTransformer<T> : EntityModelFieldTransformer
		where T : class
	{
		public IGraphReader<TypeModel, PropertyInfoField> ReadTransformed(T obj)
		{
			var reader = new ObjectGraphReaderWriter<T>(obj);
			return ReadTransformed(reader);
		}
	}

	public class EntityModelFieldTransformer<TParent, TFrom, TTo> : EntityModelFieldTransformer<TParent>
		where TParent : class
	{
		private IFieldPath<TypeModel, PropertyInfoField> _sourcePath;
		private TryConvertDelegate<TFrom, TTo> _tryConvert;
		private readonly IFieldPath<TypeModel, PropertyInfoField> _entityPath;

		public override string SourcePath { get; }

		public EntityModelFieldTransformer(
			IFieldPath<TypeModel, PropertyInfoField> sourcePath,
			IFieldPath<TypeModel, PropertyInfoField> entityPath,
			TryConvertDelegate<TFrom, TTo> tryConvert
			)
		{
			_sourcePath = sourcePath;
			_tryConvert = tryConvert;
			_entityPath = entityPath;

			SourcePath = string.Join(".", sourcePath.Fields.Select(
				q => q.FieldName
				));
		}

		public override IGraphReader<TypeModel, PropertyInfoField> ReadTransformed(
			IGraphReader<TypeModel, PropertyInfoField> graphReader
			)
		{
			if (!graphReader.CheckPath(_sourcePath))
				return null;
			var source = graphReader.Read<TFrom>(_sourcePath);
			if (!_tryConvert(source, out var converted))
				return null;
			return new ConvertedReader<TTo>(converted, _entityPath);
		}

		private class ConvertedReader<T> : ObjectGraphReaderWriterBase<T>
		{
			public ConvertedReader(T graph, IFieldPath<TypeModel, PropertyInfoField> fieldPath) :
				base(graph, fieldPath)
			{
			}
		}
	}
}
