using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.ORM.Queries;
using System.Collections.Generic;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Schema.Binding
{
	public class CreateInstanceWithNullCheck<TEntity, TKeyValue> : AssignmentBinding
	{
		private readonly string[] _readPath;
		private readonly CreateInstanceIfNull<TEntity> _impl;

		public CreateInstanceWithNullCheck(ConstructorInfo constructorInfo, string[] readPath, string[] toPath) : base(toPath)
		{
			_readPath = readPath;
			_impl = new CreateInstanceIfNull<TEntity>(constructorInfo, toPath);
		}

		public override void AssignBindingValue(IModelReadWriter from, IModelReadWriter to)
		{
			if (EqualityComparer<TKeyValue>.Default.Equals(from.ReadField<TKeyValue>(_readPath, 0), default(TKeyValue)))
				return;

			_impl.PerformBinding(from, to);
		}
	}
}
