using DtrAnonsHelper.Models;
using Microsoft.EntityFrameworkCore;

namespace DtrAnonsHelper.DataLayer.DbContext;

public class AnonsContext : Microsoft.EntityFrameworkCore.DbContext
{
    public required DbSet<Announce> Announces { get; set; }
    
    public AnonsContext(DbContextOptions<AnonsContext> options) : base(options) {}
}