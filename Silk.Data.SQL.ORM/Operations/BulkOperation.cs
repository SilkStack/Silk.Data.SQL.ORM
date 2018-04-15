using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Operations.Expressions;
using Silk.Data.SQL.Queries;

namespace Silk.Data.SQL.ORM.Operations
{
	public class BulkOperation
	{
		private readonly DataOperation[] _operations;

		public BulkOperation(DataOperation[] operations)
		{
			var bulkOperations = new List<DataOperation>();
			var currentOperations = new List<DataOperation>();
			foreach (var operation in operations)
			{
				if (!operation.CanBeBatched)
				{
					bulkOperations.Add(new BulkDataOperation(currentOperations.ToArray()));
					currentOperations.Clear();
				}
				currentOperations.Add(operation);
			}

			if (currentOperations.Count > 0)
			{
				bulkOperations.Add(new BulkDataOperation(currentOperations.ToArray()));
			}

			_operations = bulkOperations.ToArray();
		}

		public DataOperation[] GetOperations() => _operations;

		private class BulkDataOperation : DataOperation
		{
			private readonly DataOperation[] _operations;

			public override bool CanBeBatched => _operations.All(q => q.CanBeBatched);

			public BulkDataOperation(DataOperation[] operations)
			{
				_operations = operations;
			}

			public override QueryExpression GetQuery()
			{
				return new CompositeQueryExpression(
					_operations.Select(q => q.GetQuery()).ToArray()
					);
			}

			public override void ProcessResult(QueryResult queryResult)
			{
				foreach (var operation in _operations)
					operation.ProcessResult(queryResult);
			}

			public override async Task ProcessResultAsync(QueryResult queryResult)
			{
				foreach (var operation in _operations)
					await operation.ProcessResultAsync(queryResult);
			}
		}
	}

	public class BulkOperationWithResult<T> : BulkOperation
	{
		private readonly DataOperationWithResult<T> _dataOperationWithResult;

		public T Result => _dataOperationWithResult.Result;

		public BulkOperationWithResult(DataOperation[] operations, DataOperationWithResult<T> dataOperationWithResult) : base(operations)
		{
			_dataOperationWithResult = dataOperationWithResult;
		}
	}

	public class BulkOperationWithResult<T1, T2> : BulkOperation
	{
		private readonly DataOperationWithResult<T1> _dataOperationWithResult1;
		private readonly DataOperationWithResult<T2> _dataOperationWithResult2;

		public (T1,T2) Result => (_dataOperationWithResult1.Result, _dataOperationWithResult2.Result);

		public BulkOperationWithResult(DataOperation[] operations,
			DataOperationWithResult<T1> dataOperationWithResult1,
			DataOperationWithResult<T2> dataOperationWithResult2) : base(operations)
		{
			_dataOperationWithResult1 = dataOperationWithResult1;
			_dataOperationWithResult2 = dataOperationWithResult2;
		}
	}
}
