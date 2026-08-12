using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RvPersonalFinance.Api.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace RvPersonalFinance.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("rv_personal_finance_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(Options => Options.UseNpgsql(_dbContainer.GetConnectionString()));
        });
    }
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
