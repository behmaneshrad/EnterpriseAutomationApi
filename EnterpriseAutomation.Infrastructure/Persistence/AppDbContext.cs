using Microsoft.EntityFrameworkCore;
using EnterpriseAutomation.Domain.Entities;

namespace EnterpriseAutomation.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
