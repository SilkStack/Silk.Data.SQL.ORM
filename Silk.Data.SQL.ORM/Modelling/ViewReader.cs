using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using System;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class ViewReader<TView> : IGraphReader<ViewIntersectionModel, ViewIntersectionField>
		where TView : class
	{
		private readonly TView _graph;

		private IGraphReader<ViewIntersectionModel, ViewIntersectionField> _rootReader;

		public ViewReader(TView graph)
		{
			_graph = graph;
			_rootReader = new NodeReader<TView>(_graph);
		}

		private IGraphReader<ViewIntersectionModel, ViewIntersectionField> GetNodeReader(
			IFieldPath<ViewIntersectionModel, ViewIntersectionField> fieldPath
			)
		{
			int offset = 0;
			var reader = _rootReader;
			foreach (var pathSegment in fieldPath.Fields)
			{
				offset++;
				if (pathSegment is IConvertedViewField convertedViewField)
				{
					reader = convertedViewField.ConvertToReader(reader, offset);
				}
			}
			return reader;
		}

		public bool CheckContainer(IFieldPath<ViewIntersectionModel, ViewIntersectionField> fieldPath)
		{
			var reader = GetNodeReader(fieldPath);
			return reader.CheckContainer(fieldPath);
		}

		public bool CheckPath(IFieldPath<ViewIntersectionModel, ViewIntersectionField> fieldPath)
		{
			var reader = GetNodeReader(fieldPath);
			return reader.CheckPath(fieldPath);
		}

		public IGraphReaderEnumerator<ViewIntersectionModel, ViewIntersectionField> GetEnumerator<T>(
			IFieldPath<ViewIntersectionModel, ViewIntersectionField> fieldPath
			)
			=> throw new NotSupportedException();

		public T Read<T>(IFieldPath<ViewIntersectionModel, ViewIntersectionField> fieldPath)
		{
			var reader = GetNodeReader(fieldPath);
			return reader.Read<T>(fieldPath);
		}
	}

	internal class NodeReader<TNode> : IGraphReader<ViewIntersectionModel, ViewIntersectionField>
	{
		private readonly TNode _graph;
		private readonly ObjectGraphPropertyAccessor<TNode> _accessor;

		public int Offset { get; set; }

		public NodeReader(TNode graph)
		{
			_graph = graph;
			_accessor = ObjectGraphPropertyAccessor.GetFor<TNode>();
		}

		public bool CheckContainer(IFieldPath<ViewIntersectionModel, ViewIntersectionField> fieldPath)
			=> _accessor.GetPropertyChecker(fieldPath, false, Offset)(_graph);

		public bool CheckPath(IFieldPath<ViewIntersectionModel, ViewIntersectionField> fieldPath)
			=> _accessor.GetPropertyChecker(fieldPath, true, Offset)(_graph);

		public IGraphReaderEnumerator<ViewIntersectionModel, ViewIntersectionField> GetEnumerator<T>(IFieldPath<ViewIntersectionModel, ViewIntersectionField> fieldPath)
			=> throw new NotImplementedException();

		public T Read<T>(IFieldPath<ViewIntersectionModel, ViewIntersectionField> path)
			=> _accessor.GetPropertyReader<T>(path, Offset)(_graph);
	}
}
