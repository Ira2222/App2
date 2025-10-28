using App2.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App2.Tests.Integration;

public class TestingWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private SqliteConnection? _sqliteConnection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.Sources.Clear();
            configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            configurationBuilder.AddJsonFile("appsettings.Testing.json", optional: true, reloadOnChange: false);
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Features:KeyVault"] = "false",
                ["Features:Authentication"] = "false",
                ["Features:SecurityHeaders"] = "true",
                ["Features:RateLimiting"] = "true",
                ["Features:OutputCaching"] = "true",
                ["Features:CORS"] = "true",
                ["Security:DevHeader:Enabled"] = "true",
                ["Data:Provider"] = "Sqlite"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            _sqliteConnection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
            _sqliteConnection.Open();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_sqliteConnection);
            });

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _sqliteConnection?.Close();
            _sqliteConnection?.Dispose();
        }
    }
}
