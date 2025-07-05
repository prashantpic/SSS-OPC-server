using Gateways.Api.Extensions;
using Gateways.Api.Transforms;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddGatewayAuthentication(builder.Configuration); // Custom extension method
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms<CustomTransformProvider>(); // Register custom transforms

var app = builder.Build();

// 2. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Optional: Add developer-friendly diagnostics like developer exception page
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseRouting();

// These must be in the correct order: Authentication -> Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();