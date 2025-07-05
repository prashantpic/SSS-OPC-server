using MassTransit;
using Serilog;
using Serilog.Formatting.Json;
using SSS.Services.Notification.Application.Abstractions;
using SSS.Services.Notification.Application.Services;
using SSS.Services.Notification.Configuration;
using SSS.Services.Notification.Infrastructure.Channels.Abstractions;
using SSS.Services.Notification.Infrastructure.Channels.Email;
using SSS.Services.Notification.Infrastructure.Channels.Email.Abstractions;
using SSS.Services.Notification.Infrastructure.Channels.Email.Adapters;
using SSS.Services.Notification.Infrastructure.Channels.Sms;
using SSS.Services.Notification.Infrastructure.Channels.Sms.Abstractions;
using SSS.Services.Notification.Infrastructure.Channels.Sms.Adapters;
using SSS.Services.Notification.Infrastructure.Templates.Abstractions;
using SSS.Services.Notification.Infrastructure.Templates.Services;
using SSS.Services.Notification.Presentation.Consumers;

namespace SSS.Services.Notification;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console(new JsonFormatter()))
            .ConfigureServices((hostContext, services) =>
            {
                // Configure strongly-typed settings objects
                services.AddOptions<NotificationSettings>()
                    .Bind(hostContext.Configuration.GetSection(NotificationSettings.SectionName));
                services.AddOptions<EmailSettings>()
                    .Bind(hostContext.Configuration.GetSection(EmailSettings.SectionName));
                services.AddOptions<SmsSettings>()
                    .Bind(hostContext.Configuration.GetSection(SmsSettings.SectionName));

                // Register Application Services
                services.AddScoped<INotificationService, NotificationService>();

                // Register Infrastructure Services
                // Template Service
                services.AddSingleton<ITemplateService, FileSystemTemplateService>();

                // Channel Providers/Adapters
                services.AddSingleton<IEmailProvider, MailKitSmtpAdapter>();
                services.AddSingleton<ISmsProvider, TwilioSmsAdapter>();

                // Channel Strategies
                services.AddSingleton<INotificationChannel, EmailChannel>();
                services.AddSingleton<INotificationChannel, SmsChannel>();
                
                // Configure MassTransit with RabbitMQ
                services.AddMassTransit(x =>
                {
                    x.AddConsumer<AlarmEventConsumer>();
                    x.AddConsumer<HealthAlertConsumer>();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        var connectionString = hostContext.Configuration.GetConnectionString("MessageBus");
                        cfg.Host(new Uri(connectionString!));
                        
                        // Configure a default retry policy for consumers
                        cfg.UseMessageRetry(r => r
                            .Interval(3, TimeSpan.FromSeconds(5))
                            .Ignore<InvalidOperationException>()); // Don't retry on configuration errors
                        
                        cfg.ConfigureEndpoints(context);
                    });
                });
            })
            .Build();

        await host.RunAsync();
    }
}