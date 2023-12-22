namespace DtrAnonsHelper.DataLayer.Repository;

public interface IRepository<TEntity> where TEntity : class
{
    public Task<IEnumerable<TEntity>> GetAll();
    public Task BatchAdd(IEnumerable<TEntity> entities);
    public Task BatchUpdate(IEnumerable<TEntity> entities);
    public Task RemoveAll();
}