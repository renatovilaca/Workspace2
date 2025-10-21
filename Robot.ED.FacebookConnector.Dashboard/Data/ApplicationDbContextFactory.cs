using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Robot.ED.FacebookConnector.Common.Configuration;

namespace Robot.ED.FacebookConnector.Dashboard.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var appDbOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        appDbOptionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=robotfacebookconnector;Username=postgres;Password=postgres",
            b => b.MigrationsAssembly("Robot.ED.FacebookConnector.Dashboard"));
        var appDbContext = new AppDbContext(appDbOptionsBuilder.Options);

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=robotfacebookconnector;Username=postgres;Password=postgres",
            b => b.MigrationsAssembly("Robot.ED.FacebookConnector.Dashboard"));

        return new ApplicationDbContext(optionsBuilder.Options, appDbContext);
    }
}
