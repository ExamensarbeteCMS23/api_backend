using api_backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using System; // Lägg till för Environment

namespace api_backend.Contexts
{
    public class DesignFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            // Hämta miljövariabel
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Bygg konfiguration baserat på miljö
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Lägg till miljöspecifik konfiguration om den finns
            if (!string.IsNullOrEmpty(environment))
            {
                configBuilder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
            }

            var configuration = configBuilder.Build();

            // Hämta connectionstring från konfigurationen 
            var connectionString = configuration.GetConnectionString("DefaultConnectionString");

            // Om connectionstring inte hittas, använd default
            if (string.IsNullOrEmpty(connectionString))
            {
                // LocalDB skapas lokalt om den inte redan finns.
                connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ExjobbDb_Default;Trusted_Connection=True;";
                System.Console.WriteLine($"WARNING: No connection string found for environment '{environment}'. Using fallback connection.");
            }
            else
            {
                // Skriv ut vilken connectionstring som används
                System.Console.WriteLine($"Using connection string from appsettings.{environment}.json");
            }

            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new DataContext(optionsBuilder.Options);
        }
    }
}