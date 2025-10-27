using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Robot.ED.FacebookConnector.Common.Configuration;

namespace Robot.ED.FacebookConnector.Dashboard.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var appDbOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        appDbOptionsBuilder.UseNpgsql(
            connectionString,
            b => b.MigrationsAssembly("Robot.ED.FacebookConnector.Dashboard"));
        var appDbContext = new AppDbContext(appDbOptionsBuilder.Options);

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            b => b.MigrationsAssembly("Robot.ED.FacebookConnector.Dashboard"));

        return new ApplicationDbContext(optionsBuilder.Options, appDbContext);
    }
}
