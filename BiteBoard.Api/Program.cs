using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using BiteBoard.API.Helpers;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Seeds;
using Serilog;

namespace BiteBoard.API;

class Program
{
    async static Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        try
        {
            Log.Information("Starting BiteBoard server.");

            IHost host = CreateHostBuilder(args).Build();
            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                ILoggerFactory loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("BiteBoard");
                try
                {
                    var tenantInitializer = scope.ServiceProvider.GetRequiredService<TenantInitializer>();
                    await tenantInitializer.CreateDefaultTenantAsync();
                    logger.LogInformation("Application Starting");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "An error occurred seeding the DB");
                }
            }
            host.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "The server terminated unexpectedly.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog((context, loggerConfiguration) =>
            {
                loggerConfiguration.WriteTo.Console();
                loggerConfiguration.ReadFrom.Configuration(context.Configuration);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}