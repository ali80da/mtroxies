using Microsoft.EntityFrameworkCore;
using Roxi.Data.Entities.Proxi;

namespace Roxi.Data.Context;

public class RoxiDatabaseContext(DbContextOptions<RoxiDatabaseContext> options) : DbContext(options)
{


    public DbSet<Proxi> Proxies { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


    }

}