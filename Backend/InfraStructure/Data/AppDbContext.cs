using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.InfraStructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
    // DbSets by entity
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<CountryCurrency> Currencies => Set<CountryCurrency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        // Using Data Annotations on entities instead of Fluent API configurations
        base.OnModelCreating(modelBuilder);
        }
    }