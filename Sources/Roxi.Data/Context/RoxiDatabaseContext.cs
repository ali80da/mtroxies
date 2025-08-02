using Microsoft.EntityFrameworkCore;
using Roxi.Data.Entities.Proxi;

namespace Roxi.Data.Context;

public class RoxiDatabaseContext(DbContextOptions<RoxiDatabaseContext> options) : DbContext(options)
{


    public DbSet<Proxi> Proxies { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Proxi>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Port).IsRequired();
            entity.Property(e => e.Secret).IsRequired();
            entity.Property(e => e.SponsorChannel).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        });
    }

}