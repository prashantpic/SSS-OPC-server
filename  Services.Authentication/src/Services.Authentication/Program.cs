using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Opc.System.Services.Authentication.Application.Interfaces;
using Opc.System.Services.Authentication.Domain.Entities;
using Opc.System.Services.Authentication.Infrastructure.Identity;
using Opc.System.Services.Authentication.Infrastructure.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// 1. Add services to the container.

// Configure strongly-typed settings objects
builder.Services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
builder.Services.Configure<PasswordOptions>(configuration.GetSection("PasswordPolicy"));


// Configure Database
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("AuthDatabase")));

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        // Use settings from appsettings.json for password policy
        var passwordPolicy = configuration.GetSection("PasswordPolicy").Get<PasswordOptions>() ?? new PasswordOptions();
        options.Password.RequiredLength = passwordPolicy.RequiredLength;
        options.Password.RequireDigit = passwordPolicy.RequireDigit;
        options.Password.RequireLowercase = passwordPolicy.RequireLowercase;
        options.Password.RequireUppercase = passwordPolicy.RequireUppercase;
        options.Password.RequireNonAlphanumeric = passwordPolicy.RequireNonAlphanumeric;

        options.User.RequireUniqueEmail = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// Configure Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });
    // External provider configuration would go here if needed, e.g.:
    // .AddOpenIdConnect("Google", options => { ... });

// Configure MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Register application services and repositories
builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();
// IAuditService will be implemented later, for now a placeholder/dummy service
builder.Services.AddScoped<IAuditService, Infrastructure.Services.AuditService>();
// Other services
// builder.Services.AddScoped<IUserRepository, UserRepository>(); - Note: UserManager/Identity handles most of this.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Services.Authentication API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. <br/> 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      <br/> Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});


var app = builder.Build();

// 2. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed database on startup (optional but recommended for dev)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await dbContext.Database.MigrateAsync();
    // You could add more complex seeding logic here if needed.
}


app.Run();


// Dummy service implementations for compilation
namespace Opc.System.Services.Authentication.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly ILogger<AuditService> _logger;
        // In a real implementation, you would inject AuthDbContext here.

        public AuditService(ILogger<AuditService> logger)
        {
            _logger = logger;
        }

        public Task LogEventAsync(string eventType, string outcome, object details, Guid? actingUserId = null, string? subjectId = null)
        {
            _logger.LogInformation("AUDIT: {EventType} | Outcome: {Outcome} | Actor: {ActingUserId} | Subject: {SubjectId} | Details: {@Details}",
                eventType, outcome, actingUserId, subjectId, details);
            // In a real implementation, you would create an AuditLog entity and save it to the database.
            return Task.CompletedTask;
        }
    }
}

// Settings classes
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryInMinutes { get; set; }
}