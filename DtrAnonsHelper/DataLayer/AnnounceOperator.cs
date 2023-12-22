using DtrAnonsHelper.DataLayer.Repository;
using DtrAnonsHelper.Models;

namespace DtrAnonsHelper.DataLayer;

public class AnnounceOperator
{
    private IRepository<Announce> _repository;

    public AnnounceOperator(IRepository<Announce> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Announce>> GetAll()
    {
        var announces = await _repository.GetAll();
        return announces;
    }

    public async Task UpdateBatch(IEnumerable<Announce> announces)
    {
        await _repository.BatchUpdate(announces);
    }

    public async Task RemoveAll()
    {
        await _repository.RemoveAll();
    }

    public async Task SaveBatch(IEnumerable<Announce> announces)
    {
        await _repository.BatchAdd(announces);
    }
}