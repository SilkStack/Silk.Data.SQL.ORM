namespace Silk.Data.SQL.ORM.Schema
{
	public abstract class BuiltEntityField
	{
		public IEntityField EntityField { get; }

		public BuiltEntityField(IEntityField entityField)
		{
			EntityField = entityField;
		}

		public abstract ForeignKey BuildForeignKey(string propertyPathPrefix, string[] modelPath, string columnName = null);

		public abstract ProjectionField BuildProjectionField(string sourceName, string fieldName,
			string aliasName, string[] modelPath, EntityFieldJoin join);
	}

	public class BuiltEntityField<TEntity> : BuiltEntityField
	{
		public BuiltEntityField(IEntityFieldOfEntity<TEntity> entityField) :
			base(entityField)
		{
		}

		public override ForeignKey BuildForeignKey(string propertyPathPrefix, string[] modelPath, string columnName = null)
		{
			throw new System.NotImplementedException();
		}

		public override ProjectionField BuildProjectionField(string sourceName, string fieldName, string aliasName, string[] modelPath, EntityFieldJoin join)
		{
			throw new System.NotImplementedException();
		}
	}

	public class BuiltEntityField<TValue, TEntity> : BuiltEntityField
	{
		public BuiltEntityField(EntityField<TValue, TEntity> entityField) :
			base(entityField)
		{
		}

		public override ForeignKey BuildForeignKey(string propertyPathPrefix, string[] modelPath, string columnName = null)
		{
			throw new System.NotImplementedException();
		}

		public override ProjectionField BuildProjectionField(string sourceName, string fieldName, string aliasName, string[] modelPath, EntityFieldJoin join)
		{
			throw new System.NotImplementedException();
		}
	}
}
