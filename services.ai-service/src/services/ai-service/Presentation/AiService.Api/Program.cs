using AiService.Api.Extensions;

/// <summary>
/// Main entry point for the AI Service application.
/// Bootstraps and runs the AI microservice application.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container.
        // 1. Register services from the Application layer (e.g., MediatR handlers)
        builder.Services.AddApplicationServices();
        
        // 2. Register services from the Infrastructure layer (e.g., DB context, repositories, clients)
        builder.Services.AddInfrastructureServices(builder.Configuration);

        // 3. Add Presentation-layer services
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Version = "v1",
                Title = "AI Service API",
                Description = "API for the SSS-OPC-Client AI and Machine Learning functionalities."
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Service API v1");
                options.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
            });
        }

        app.UseHttpsRedirection();

        // app.UseAuthentication(); // If auth is added
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}