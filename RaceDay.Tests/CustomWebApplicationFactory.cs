using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.API.Data;

namespace RaceDay.Tests
{
    /// <summary>
    /// Boots the real RaceDay.API pipeline (Program.cs) for integration-style
    /// tests, but replaces the SQL Server DbContext registration with an
    /// EF Core InMemory database, so tests don't need a real SQL Server
    /// instance - this is what lets the CI workflow run the tests on
    /// GitHub's runners without any database set up.
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public string DatabaseName { get; } = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(DatabaseName);
                });
            });
        }
    }
}
