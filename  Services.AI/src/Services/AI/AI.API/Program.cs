using Microsoft.Extensions.DependencyInjection;
using Opc.System.Services.AI.Application;
using Opc.System.Services.AI.Infrastructure;

namespace Opc.System.Services.AI.API;

/// <summary>
/// Main entry point for the AI microservice. This file configures and launches the ASP.NET Core host.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        // Register services from Application and Infrastructure layers
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.Configuration);

        builder.Services.AddControllers();
        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Version = "v1",
                Title = "Services.AI API",
                Description = "Microservice for all backend Artificial Intelligence (AI) and Machine Learning (ML) functionalities.",
            });
        });

        // Configure health checks
        builder.Services.AddHealthChecks();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();
        
        // Map health check endpoint
        app.MapHealthChecks("/healthz");

        app.Run();
    }
}