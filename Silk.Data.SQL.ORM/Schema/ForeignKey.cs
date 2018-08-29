using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Queries;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class ForeignKey
	{
		public abstract Column LocalColumn { get; }
		public abstract Column ForeignColumn { get; }
		public abstract string[] ModelPath { get; }

		public abstract IValueReader CreateValueReader(IModelReadWriter writeToModelReadWriter);

		public abstract ProjectionField BuildProjectionField(string sourceName, string fieldName, string aliasName, string[] modelPath);
	}

	public class ForeignKey<TEntity, TValue> : ForeignKey
	{
		public override Column LocalColumn { get; }
		public override Column ForeignColumn { get; }
		public override string[] ModelPath { get; }

		public ForeignKey(Column localColumn, Column foreignColumn,
			string[] modelPath)
		{
			LocalColumn = localColumn;
			ForeignColumn = foreignColumn;
			ModelPath = modelPath;
		}

		public override IValueReader CreateValueReader(IModelReadWriter writeToModelReadWriter)
		{
			return new ValueReader(writeToModelReadWriter, ModelPath);
		}

		public override ProjectionField BuildProjectionField(string sourceName, string fieldName, string aliasName, string[] modelPath)
		{
			return new ProjectionField<TEntity, TValue>(sourceName, fieldName, aliasName, modelPath, null);
		}

		private class ValueReader : IValueReader
		{
			private readonly IModelReadWriter _objectReadWriter;
			private readonly string[] _modelPath;

			public ValueReader(IModelReadWriter objectReadWriter, string[] modelPath)
			{
				_objectReadWriter = objectReadWriter;
				_modelPath = modelPath;
			}

			public object Read()
			{
				return _objectReadWriter.ReadField<TValue>(_modelPath, 0);
			}
		}
	}
}
