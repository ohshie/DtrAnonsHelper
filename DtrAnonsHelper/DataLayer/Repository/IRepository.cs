namespace DtrAnonsHelper.DataLayer.Repository;

public interface IRepository<TEntity> where TEntity : class
{
    public Task<TEntity> Get(int id);
    public Task<TEntity> GetByChannel(string channelName);
    public Task<IEnumerable<TEntity>> GetAll();
    public Task Add(TEntity entity);
    public Task BatchAdd(IEnumerable<TEntity> entities);
    public Task Update(TEntity entity);
    public Task BatchUpdate(IEnumerable<TEntity> entities);
    public Task Remove(TEntity entity);
    public Task RemoveAll();
}