using System.Linq;

namespace Silk.Data.SQL.ORM.Operations
{
	public abstract class OperationBuilderBase
	{
		protected DataOperation[] DataOperations { get; }

		protected OperationBuilderBase(DataOperation[] dataOperations)
		{
			DataOperations = dataOperations;
		}
	}

	public class OperationBuilder : OperationBuilderBase
	{
		public OperationBuilder() : base(new DataOperation[0])
		{
		}

		private OperationBuilder(DataOperation[] dataOperations) : base(dataOperations)
		{
		}

		public OperationBuilder Add(DataOperation dataOperation)
		{
			return new OperationBuilder(DataOperations.Concat(new[] { dataOperation }).ToArray());
		}

		public OperationBuilder<T> Add<T>(DataOperationWithResult<T> dataOperation)
		{
			return new OperationBuilder<T>(DataOperations.Concat(new DataOperation[] { dataOperation }).ToArray(), dataOperation);
		}

		public BulkOperation Build()
		{
			return new BulkOperation(DataOperations);
		}
	}

	public class OperationBuilder<TResult> : OperationBuilderBase
	{
		private DataOperationWithResult<TResult> _resultOperation;

		public OperationBuilder(DataOperation[] operations, DataOperationWithResult<TResult> resultOperation) :
			base(operations)
		{
			_resultOperation = resultOperation;
		}

		public OperationBuilder<TResult> Add(DataOperation dataOperation)
		{
			return new OperationBuilder<TResult>(
				DataOperations.Concat(new[] { dataOperation }).ToArray(),
				_resultOperation
				);
		}

		public OperationBuilder<TResult, T> Add<T>(DataOperationWithResult<T> dataOperation)
		{
			return new OperationBuilder<TResult, T>(DataOperations.Concat(
				new DataOperation[] { dataOperation }
				).ToArray(), _resultOperation, dataOperation);
		}

		public BulkOperationWithResult<TResult> Build()
		{
			return new BulkOperationWithResult<TResult>(DataOperations, _resultOperation);
		}
	}

	public class OperationBuilder<TResult1, TResult2> : OperationBuilderBase
	{
		private DataOperationWithResult<TResult1> _resultOperation1;
		private DataOperationWithResult<TResult2> _resultOperation2;

		public OperationBuilder(DataOperation[] operations,
			DataOperationWithResult<TResult1> resultOperation1,
			DataOperationWithResult<TResult2> resultOperation2) :
			base(operations)
		{
			_resultOperation1 = resultOperation1;
			_resultOperation2 = resultOperation2;
		}

		public OperationBuilder<TResult1, TResult2> Add(DataOperation dataOperation)
		{
			return new OperationBuilder<TResult1,TResult2>(
				DataOperations.Concat(new[] { dataOperation }).ToArray(),
				_resultOperation1, _resultOperation2
				);
		}

		public BulkOperationWithResult<TResult1,TResult2> Build()
		{
			return new BulkOperationWithResult<TResult1, TResult2>(DataOperations, _resultOperation1, _resultOperation2);
		}
	}
}
