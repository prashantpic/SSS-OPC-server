using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Opc.System.Services.Integration.API.Workers;
using Opc.System.Services.Integration.Application.Contracts.External;
using Opc.System.Services.Integration.Application.Contracts.Infrastructure;
using Opc.System.Services.Integration.Application.Contracts.Persistence;
using Opc.System.Services.Integration.Infrastructure.External.AR;
using Opc.System.Services.Integration.Infrastructure.External.Blockchain;
using Opc.System.Services.Integration.Infrastructure.External.DigitalTwin;
using Opc.System.Services.Integration.Infrastructure.External.Iot;
using Opc.System.Services.Integration.Infrastructure.Persistence; // Placeholder for repository implementation
using Opc.System.Services.Integration.Infrastructure.Services;
using Serilog;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

// Add services to the DI container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT Bearer Authentication (details would be in appsettings.json)
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.Audience = builder.Configuration["Jwt:Audience"];
    });
builder.Services.AddAuthorization();


// Register MediatR and scan the Application assembly for handlers
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IIotPlatformConnector).Assembly));

// Register application and infrastructure services
// Register repositories (using a placeholder in-memory implementation for now)
builder.Services.AddSingleton<IIntegrationConnectionRepository, InMemoryIntegrationConnectionRepository>();
builder.Services.AddSingleton<IDataMapRepository, InMemoryDataMapRepository>();

// Register external service adapters as singletons
builder.Services.AddSingleton<IIotPlatformConnector, MqttIotPlatformConnector>();
builder.Services.AddSingleton<IBlockchainAdapter, NethereumBlockchainAdapter>();
builder.Services.AddSingleton<IDigitalTwinAdapter, AzureDigitalTwinAdapter>();
builder.Services.AddSingleton<IArDataStreamer, WebSocketArDataStreamer>();

// Register other services
builder.Services.AddScoped<IDataTransformer, JsonDataTransformer>();

// Register background workers
builder.Services.AddHostedService<BlockchainQueueWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseWebSockets(); // Enable WebSocket middleware

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Define a WebSocket endpoint for Augmented Reality streaming
app.Map("/ws/ar", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var deviceId = context.Request.Query["deviceId"].ToString();
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("A 'deviceId' query parameter is required.");
            return;
        }

        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var streamer = context.RequestServices.GetRequiredService<IArDataStreamer>() as WebSocketArDataStreamer;
        
        if (streamer == null)
        {
             context.Response.StatusCode = StatusCodes.Status500InternalServerError;
             await context.Response.WriteAsync("AR Streamer service is not available.");
             return;
        }
        
        streamer.AddSocket(deviceId, webSocket);

        try
        {
            // Keep the connection alive, wait for close message
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            } while (!result.CloseStatus.HasValue);
            
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
        catch(Exception ex)
        {
            Log.Error(ex, "Error in AR WebSocket connection for device {DeviceId}", deviceId);
        }
        finally
        {
            await streamer.RemoveSocket(deviceId);
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});


app.Run();