using App2.Domain.Abstractions;
using App2.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace App2.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Todo> Todos => Set<Todo>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditing();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditing();
        return base.SaveChanges();
    }

    private void ApplyAuditing()
    {
        var now = DateTimeOffset.UtcNow;
        var http = _httpContextAccessor?.HttpContext;
        var user = http?.User?.Identity?.Name ?? "system";
        var ip = http?.Connection?.RemoteIpAddress?.ToString();

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = user;
                entry.Entity.CreatedByIp = ip;
                entry.Entity.ModifiedAt = now;
                entry.Entity.ModifiedBy = user;
                entry.Entity.ModifiedByIp = ip;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = now;
                entry.Entity.ModifiedBy = user;
                entry.Entity.ModifiedByIp = ip;
            }
        }
    }
}
