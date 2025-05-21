using AIService.Configuration;
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.IO;
using AIService.Api.GrpcServices; // Assuming this is where AiProcessingGrpcService will be

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            // Configure Serilog from appsettings.json, etc.
            // For a real application, integrate with REPO-SHARED-UTILS for logging
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting AI-Processing-Service host");

            var builder = WebApplication.CreateBuilder(args);

            // Integrate Serilog with ASP.NET Core logging
            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console());

            // Add services to the container.
            builder.Services.AddAIServices(builder.Configuration);

            builder.Services.AddControllers();
            builder.Services.AddGrpc();

            // Configure Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AI-Processing-Service API", Version = "v1" });
                // REQ-SAP-005: API Documentation. Include XML comments if configured in .csproj
                // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                // if (File.Exists(xmlPath))
                // {
                //    c.IncludeXmlComments(xmlPath);
                // }
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI-Processing-Service API v1"));
            }

            // app.UseHttpsRedirection(); // Consider if running behind a reverse proxy handling TLS

            app.UseRouting();

            // app.UseAuthentication(); // To be configured if REPO-SECURITY integration is handled here
            // app.UseAuthorization();

            app.MapControllers(); // REQ-SAP-002: RESTful API endpoints
            app.MapGrpcService<AiProcessingGrpcService>(); // REQ-SAP-003: gRPC services. Placeholder for actual service.

            app.Run();
        }
        catch (System.Exception ex)
        {
            Log.Fatal(ex, "AI-Processing-Service host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}