using Microsoft.EntityFrameworkCore;
using EnterpriseAutomation.Domain.Entities;

namespace EnterpriseAutomation.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Request> Requests => Set<Request>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().
            HasMany(r => r.Requests)
            .WithOne(u => u.User)
            .HasForeignKey(r => r.CreatedByUserId);
    }
}
