namespace Silk.Data.SQL.ORM
{
	public interface IEntityTable<T>
		where T : class
	{
		IDeferred CreateTable();
		IDeferred DropTable();
		IDeferred TableExists(out DeferredResult<bool> tableExists);
	}
}
