using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping.Binding;
using System.Collections.Generic;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Schema.Binding
{
	public class CreateInstanceWithNullCheck<TEntity, TKeyValue> : AssignmentBinding
	{
		private readonly CreateInstanceIfNull<TEntity> _impl;

		public CreateInstanceWithNullCheck(ConstructorInfo constructorInfo, string[] readPath, IFieldReference toField)
			: base(toField)
		{
			_impl = new CreateInstanceIfNull<TEntity>(constructorInfo, toField);
		}

		public override void AssignBindingValue(IModelReadWriter from, IModelReadWriter to)
		{
			if (EqualityComparer<TKeyValue>.Default.Equals(from.ReadField<TKeyValue>(To), default(TKeyValue)))
				return;

			_impl.PerformBinding(from, to);
		}
	}
}
