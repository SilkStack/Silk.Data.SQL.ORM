namespace Silk.Data.SQL.ORM
{
	public sealed class PrimaryKeyEntityReference<TEntity> : IEntityReference<TEntity>
		where TEntity : class
	{
		private readonly TEntity _referenceEntity;

		internal PrimaryKeyEntityReference(TEntity entity)
		{
			_referenceEntity = entity;
		}

		public TEntity AsEntity()
			=> _referenceEntity;
	}
}
