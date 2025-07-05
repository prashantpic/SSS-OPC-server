using AiService.Domain.Aggregates.AiModel;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AiService.Infrastructure.Persistence;

public class AiServiceDbContext : DbContext
{
    public AiServiceDbContext(DbContextOptions<AiServiceDbContext> options) : base(options)
    {
    }

    public DbSet<AiModel> AiModels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}