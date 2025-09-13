using BiteBoard.Data.Contexts;
using BiteBoard.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BiteBoard.Api.Factory
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Build configuration to read connection string
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Get connection string from configuration
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Configure the database provider
            optionsBuilder.UseNpgsql(connectionString);

            // Use the simplest constructor for design-time
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }

    //// Mock services for design-time (if needed for testing)
    //public class DesignTimeServices
    //{
    //    public class MockDateTimeService : IDateTimeService
    //    {
    //        public DateTime NowUtc => DateTime.UtcNow;
    //    }

    //    public class MockAuthenticatedUserService : IAuthenticatedUserService
    //    {
    //        public Guid UserId => Guid.Empty;

    //        public string Username => String.Empty;
    //    }
    //}
}
