using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Robot.ED.FacebookConnector.Common.Configuration;

namespace Robot.ED.FacebookConnector.Service.API.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=robotfacebookconnector;Username=postgres;Password=postgres",
            b => b.MigrationsAssembly("Robot.ED.FacebookConnector.Service.API"));

        return new AppDbContext(optionsBuilder.Options);
    }
}
