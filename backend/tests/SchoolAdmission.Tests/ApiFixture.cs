using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace SchoolAdmission.Tests;

/// <summary>
/// Starts one throwaway PostgreSQL container for the whole test run and hands out
/// an HttpClient wired to the real API. No manual setup, no shared dev database.
/// </summary>
public sealed class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private readonly List<WebApplicationFactory<Program>> _factories = [];

    public async Task InitializeAsync() => await _db.StartAsync();

    /// <summary>
    /// Creates a factory pointing at the test database. Pass holdSeconds to override
    /// HOLD_DURATION_SECONDS for that app instance (used by the expiry test).
    /// </summary>
    public HttpClient CreateClient(int? holdSeconds = null)
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", _db.GetConnectionString());
            if (holdSeconds is int seconds)
                builder.UseSetting("HOLD_DURATION_SECONDS", seconds.ToString());
        });

        _factories.Add(factory);
        return factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        foreach (var factory in _factories)
            await factory.DisposeAsync();

        await _db.DisposeAsync();
    }
}

[CollectionDefinition("api")]
public class ApiCollection : ICollectionFixture<ApiFixture>;
