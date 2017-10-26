using Silk.Data.Modelling.Bindings;

namespace Silk.Data.SQL.ORM.Modelling.Bindings
{
	public class PrimaryKeyBinding : ModelBinding
	{
		public PrimaryKeyBinding(BindingDirection bindingDirection, string[] modelFieldPath,
			string[] viewFieldPath) : base(modelFieldPath, viewFieldPath)
		{
			Direction = bindingDirection;
		}

		public override BindingDirection Direction { get; }
	}
}
