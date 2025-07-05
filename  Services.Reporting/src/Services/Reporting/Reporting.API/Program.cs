using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Opc.System.Services.Reporting.Application.Abstractions;
using Opc.System.Services.Reporting.Application.Common.Behaviors;
using Opc.System.Services.Reporting.Application.Features.Distribution;
using Opc.System.Services.Reporting.Application.Features.Schedules;
using Opc.System.Services.Reporting.Infrastructure;
using Opc.System.Services.Reporting.Infrastructure.Clients;
using Opc.System.Services.Reporting.Infrastructure.Distribution;
using Opc.System.Services.Reporting.Infrastructure.Persistence;
using Opc.System.Services.Reporting.Infrastructure.ReportGeneration;
using Opc.System.Services.Reporting.Infrastructure.Scheduling;
using Opc.System.Services.Reporting.Infrastructure.Storage;
using Quartz;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the DI container.
var applicationAssembly = Assembly.GetAssembly(typeof(IDataServiceClient)); // Gets Reporting.Application assembly

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure MediatR and FluentValidation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly!));
builder.Services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Configure Infrastructure Services
builder.Services.AddDbContext<ReportingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ReportingDb")));

// Repositories
builder.Services.AddScoped<IReportTemplateRepository, ReportTemplateRepository>();
builder.Services.AddScoped<IGeneratedReportRepository, GeneratedReportRepository>();
builder.Services.AddScoped<IReportScheduleRepository, ReportScheduleRepository>();

// External Service Clients
builder.Services.AddHttpClient<IDataServiceClient, DataServiceClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ServiceEndpoints:DataServiceUrl"]!));
builder.Services.AddHttpClient<IAiServiceClient, AiServiceClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ServiceEndpoints:AiServiceUrl"]!));

// Report Generation (Strategy Pattern)
builder.Services.AddSingleton<IReportGenerationEngine, ReportGenerationEngine>();
builder.Services.AddSingleton<IReportFormatGenerator, PdfGenerator>();
builder.Services.AddSingleton<IReportFormatGenerator, ExcelGenerator>();
// Add HtmlGenerator here when implemented

// Distribution and Storage
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>(); // Could be swapped with AzureBlobStorageService
builder.Services.AddScoped<IReportDistributionService, ReportDistributionService>();

// Configure Quartz.NET for scheduling
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
    // Register the job. Scoped lifetime is managed by DI job factory.
    var jobKey = new JobKey(nameof(ReportGenerationJob), "ReportGeneration");
    q.AddJob<ReportGenerationJob>(opts => opts.WithIdentity(jobKey));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Register Hosted Services
builder.Services.AddHostedService<ReportSchedulingService>();
// builder.Services.AddHostedService<ReportRetentionService>(); // Uncomment when implemented

// Configure Authentication & Authorization
// builder.Services.AddAuthentication(...)
// builder.Services.AddAuthorization(...)

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
// app.UseCustomExceptionHandler(); // Custom middleware for standardized error responses
app.UseHttpsRedirection();
// app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed data or run migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
    // await dbContext.Database.MigrateAsync();
}

app.Run();