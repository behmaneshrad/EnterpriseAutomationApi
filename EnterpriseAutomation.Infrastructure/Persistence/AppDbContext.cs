using Microsoft.EntityFrameworkCore;
using EnterpriseAutomation.Domain.Entities;

namespace EnterpriseAutomation.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Request> Requests => Set<Request>();

    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }

    public DbSet<WorkflowStep> WorkflowSteps { get; set; }

    public DbSet<ApprovalStep> ApprovalSteps { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // Only configures AppDbContext configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    }
}
