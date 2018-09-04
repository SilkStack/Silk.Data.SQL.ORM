using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Queries;

namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class ForeignKey
	{
		public abstract Column LocalColumn { get; }
		public abstract Column ForeignColumn { get; }
		public abstract string[] ModelPath { get; }

		public abstract object ReadValue(IModelReadWriter writeToModelReadWriter, int offset);

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

		public override ProjectionField BuildProjectionField(string sourceName, string fieldName, string aliasName, string[] modelPath)
		{
			return new ProjectionField<TEntity, TValue>(sourceName, fieldName, aliasName, modelPath, null);
		}

		public override object ReadValue(IModelReadWriter writeToModelReadWriter, int offset)
		{
			return writeToModelReadWriter.ReadField<TValue>(ModelPath, offset);
		}
	}
}
