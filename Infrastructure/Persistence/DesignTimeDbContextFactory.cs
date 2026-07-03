using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("../Api/appsettings.json")
            .AddJsonFile("../Api/appsettings.Development.json", true)
            .Build();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>();

        options.UseSqlServer(
            configuration.GetConnectionString("SqlServer"));

        return new ApplicationDbContext(options.Options);
    }
}
