using Azure.Storage.Blobs;
using InfluxDB.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Opc.System.Infrastructure.Data.Abstractions;
using Opc.System.Infrastructure.Data.BlobStorage.Repositories;
using Opc.System.Infrastructure.Data.Relational;
using Opc.System.Infrastructure.Data.Relational.Repositories;
using Opc.System.Infrastructure.Data.Services;
using Opc.System.Infrastructure.Data.TimeSeries.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Defines extension methods for IServiceCollection to encapsulate the registration of all data access components,
    /// making the layer self-contained and easy to integrate.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds the Infrastructure.Data services to the specified IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="configuration">The application configuration for accessing connection strings and options.</param>
        /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
        public static IServiceCollection AddInfrastructureData(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Configure Options
            services.Configure<TimeSeriesDbOptions>(configuration.GetSection("TimeSeriesDbOptions"));
            services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorageOptions"));

            // 2. Register Relational Database (PostgreSQL)
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));

            // Register IUnitOfWork to resolve to the AppDbContext instance for the current scope.
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

            // 3. Register External Service Clients as Singletons
            services.AddSingleton(sp => 
                new InfluxDBClient(
                    configuration.GetConnectionString("InfluxDBConnection"), 
                    configuration["TimeSeriesDbOptions:Token"]
                ));
            
            services.AddSingleton(sp => 
                new BlobServiceClient(configuration.GetConnectionString("AzureBlobStorage")));

            // 4. Register Repositories with Scoped Lifetime
            services.AddScoped<IHistoricalDataRepository, HistoricalDataRepository>();
            services.AddScoped<IAlarmEventRepository, AlarmEventRepository>();
            services.AddScoped<IAiArtifactRepository, AiArtifactRepository>();
            services.AddScoped<IBlockchainLogRepository, BlockchainLogRepository>();
            
            // Register other relational repositories here, e.g.:
            // services.AddScoped<IUserRepository, UserRepository>();
            
            // 5. Register Cross-Cutting Services
            services.AddScoped<IDataMaskingService, DataMaskingService>();

            return services;
        }
    }
}