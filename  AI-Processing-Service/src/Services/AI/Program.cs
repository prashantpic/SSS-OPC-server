using AIService.Configuration;
using AIService.Api.GrpcServices; // Assuming this will be created for AiProcessingGrpcService
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace AIService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                // .WriteTo.File("logs/aiservice-.log", rollingInterval: RollingInterval.Day) // Example file sink
                .CreateLogger();
            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddGrpc();
            builder.Services.AddGrpcReflection(); // For gRPC service discovery

            // Configure strongly-typed options
            builder.Services.Configure<ModelOptions>(builder.Configuration.GetSection("ModelOptions"));
            builder.Services.Configure<NlpProviderOptions>(builder.Configuration.GetSection("NlpProviderOptions"));
            builder.Services.Configure<MLOpsOptions>(builder.Configuration.GetSection("MLOpsOptions"));

            // Add application and infrastructure services
            builder.Services.AddApplicationServices(builder.Configuration);
            builder.Services.AddInfrastructureServices(builder.Configuration);


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AI Processing Service API", Version = "v1" });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Processing Service API v1"));
                app.MapGrpcReflectionService(); // Expose gRPC reflection in development
            }

            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging(); // Log HTTP requests

            app.UseRouting();

            // Placeholder for Authentication and Authorization middleware
            // app.UseAuthentication();
            // app.UseAuthorization();

            app.MapControllers();
            app.MapGrpcService<AiProcessingGrpcService>(); // Assuming AiProcessingGrpcService will be in AIService.Api.GrpcServices

            app.Run();
        }
    }
}