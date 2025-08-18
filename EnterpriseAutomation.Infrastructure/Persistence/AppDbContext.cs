using Microsoft.EntityFrameworkCore;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Infrastructure.Persistence.Configurations;

namespace EnterpriseAutomation.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
    public DbSet<WorkflowStep> WorkflowSteps { get; set; }
    public DbSet<ApprovalStep> ApprovalSteps { get; set; }
    public DbSet<Role> Roles { get; set; }

    // Permission system
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermissions> RolePermissions { get; set; } // نام صحیح

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // کانفیگوریشن‌های موجود
        modelBuilder.ApplyConfiguration(new RequestConfiguration());
        modelBuilder.ApplyConfiguration(new ApprovalStepConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowDefinitionConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowStepConfiguration());

        // کانفیگوریشن‌های جدید
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionsConfiguration());
    }
}