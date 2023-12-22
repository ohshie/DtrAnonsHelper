using DtrAnonsHelper.DataLayer.DbContext;
using DtrAnonsHelper.Models;
using Microsoft.EntityFrameworkCore;

namespace DtrAnonsHelper.DataLayer.Repository;

public class AnonsRepository : IRepository<Announce>
{
    private readonly AnonsContext _anonsContext;

    public AnonsRepository(AnonsContext anonsContext)
    {
        _anonsContext = anonsContext;
    }

    public async Task<IEnumerable<Announce>> GetAll()
    {
        var announces = _anonsContext.Announces.AsEnumerable();
        return announces;
    }

    public Task Add(Announce entity)
    {
        throw new NotImplementedException();
    }

    public async Task BatchAdd(IEnumerable<Announce> entities)
    {
        await _anonsContext.Announces.AddRangeAsync(entities);
        await _anonsContext.SaveChangesAsync();
    }

    public async Task BatchUpdate(IEnumerable<Announce> entities)
    {
        _anonsContext.Announces.UpdateRange(entities);
        await _anonsContext.SaveChangesAsync();
    }

    public async Task RemoveAll()
    {
        await _anonsContext.Announces.ExecuteDeleteAsync();
        await _anonsContext.SaveChangesAsync();
    }
}