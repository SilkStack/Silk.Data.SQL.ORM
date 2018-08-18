using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping;
using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.ORM.Schema;
using System;
using System.Linq;
using System.Reflection;

namespace Silk.Data.SQL.ORM.Modelling
{
	public interface IProjectionModel : IModel<IEntityField>, IModel
	{
		Table EntityTable { get; }
		Mapping Mapping { get; }
		new IEntityField[] Fields { get; }

		AssignmentBinding GetCreateInstanceAsNeededBinding(string[] path);
	}

	public class ProjectionModel<T> : ModelBase<IEntityField>, IProjectionModel
	{
		public Table EntityTable { get; }
		public Mapping Mapping { get; }
		public override IEntityField[] Fields { get; }

		public ProjectionModel(IEntityField[] fields, Table entityTable, Mapping mapping)
		{
			EntityTable = entityTable;
			Fields = fields;
			Mapping = mapping;
		}

		public AssignmentBinding GetCreateInstanceAsNeededBinding(string[] path)
		{
			return new CreateInstanceIfNull<T>(GetConstructor(typeof(T)), path);
		}

		private static ConstructorInfo GetConstructor(Type type)
		{
			var ctor = type.GetTypeInfo().DeclaredConstructors
				.FirstOrDefault(q => q.GetParameters().Length == 0);
			if (ctor == null)
			{
				throw new MappingRequirementException($"A constructor with 0 parameters is required on type {type}.");
			}
			return ctor;
		}
	}
}
