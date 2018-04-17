using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Silk.Data.SQL.ORM.Modelling.Binding
{
	public class ConditionalCopyBinding : IMappingBindingFactory
	{
		public MappingBinding CreateBinding<TFrom, TTo>(ISourceField fromField, ITargetField toField)
		{
			if (typeof(TFrom) != typeof(TTo))
				throw new InvalidOperationException("TFrom and TTo type mismatch.");
			return new ConditionalCopyBinding<TFrom>(fromField.FieldPath, toField.FieldPath, toField.CanWrite);
		}
	}

	public class ConditionalCopyBinding<T> : MappingBinding
	{
		private readonly bool _canWrite;

		public ConditionalCopyBinding(string[] fromPath, string[] toPath, bool canWrite)
			: base(fromPath, toPath)
		{
			_canWrite = canWrite;
		}

		public override void CopyBindingValue(IModelReadWriter from, IModelReadWriter to)
		{
			if (!_canWrite)
				return;
			to.WriteField<T>(ToPath, 0, from.ReadField<T>(FromPath, 0));
		}
	}
}
