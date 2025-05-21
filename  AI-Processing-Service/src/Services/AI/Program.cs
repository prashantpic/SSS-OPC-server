using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using AIService.Configuration;
using AIService.Api.GrpcServices; // Assuming AiProcessingGrpcService will be in this namespace
using System;

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
                // Add other sinks like File, Seq, etc. as needed from REPO-SHARED-UTILS or configuration
                .CreateBootstrapLogger(); // Use CreateBootstrapLogger for early logging

            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console());

            // Add services to the container.
            builder.Services.AddApplicationServices(builder.Configuration);

            builder.Services.AddControllers();
            builder.Services.AddGrpc();
            builder.Services.AddGrpcReflection(); // For gRPC client tools like grpcurl

            // Configure Swagger/OpenAPI
            // REQ-SAP-005: API Documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "AI Processing Service API",
                    Version = "v1",
                    Description = "API for Predictive Maintenance, Anomaly Detection, and NLP Integration."
                });
                // Add XML comments path if configured in .csproj
                // var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                // var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                // if (System.IO.File.Exists(xmlPath))
                // {
                //    c.IncludeXmlComments(xmlPath);
                // }
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Processing Service API V1");
                    // Potentially add gRPC reflection endpoint for Swagger if using a gRPC-Swagger tool
                });
                app.MapGrpcReflectionService(); // Expose gRPC reflection in Development
            }

            // REQ-SAP-016: Secure Communication (TLS handled by Kestrel/hosting environment)
            app.UseHttpsRedirection();

            // Placeholder for Authentication/Authorization middleware from REPO-SECURITY
            // app.UseAuthentication();
            // app.UseAuthorization();

            app.MapControllers(); // REQ-SAP-002: RESTful API endpoints

            // REQ-SAP-003: gRPC services
            // AiProcessingGrpcService will be defined in Api/GrpcServices and its .proto in Api/Proto
            // This mapping assumes AiProcessingGrpcService is registered in DI
            app.MapGrpcService<AiProcessingGrpcService>();


            app.Run();
        }
    }
}