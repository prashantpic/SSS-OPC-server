using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using services.opc.client.Domain.Abstractions;
using services.opc.client.Domain.Models;
using System.Security.Cryptography.X509Certificates;

namespace services.opc.client.Infrastructure.OpcProtocolClients.Ua;

/// <summary>
/// Concrete implementation of IOpcProtocolClient for OPC Unified Architecture (UA).
/// It handles session management, security, subscriptions, and data access for OPC UA servers.
/// </summary>
public class OpcUaProtocolClient : IOpcProtocolClient
{
    private readonly ILogger<OpcUaProtocolClient> _logger;
    private Session? _session;
    private SubscriptionManager? _subscriptionManager;
    private OpcConnectionSettings? _settings;
    private ApplicationConfiguration? _appConfig;

    /// <inheritdoc/>
    public event Action<DataPoint>? OnDataReceived;
    /// <inheritdoc/>
    public event Action<AlarmEvent>? OnAlarmReceived;

    public OpcUaProtocolClient(ILogger<OpcUaProtocolClient> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task ConnectAsync(OpcConnectionSettings settings, CancellationToken cancellationToken)
    {
        _settings = settings;
        _logger.LogInformation("Connecting to OPC UA server {ServerId} at {EndpointUrl}", settings.ServerId, settings.EndpointUrl);

        try
        {
            _appConfig = await CreateApplicationConfiguration();
            var endpointDescription = CoreClientUtils.SelectEndpoint(_settings.EndpointUrl, useSecurity: true);
            var endpointConfiguration = EndpointConfiguration.Create(_appConfig);
            var configuredEndpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

            _session = await Session.Create(
                _appConfig,
                configuredEndpoint,
                updateBeforeConnect: false,
                checkDomain: false,
                sessionName: $"{_appConfig.ApplicationName} - {settings.ServerId}",
                sessionTimeout: 60000,
                identity: new UserIdentity(),
                preferredLocales: null
            );

            _logger.LogInformation("OPC UA session created for {ServerId}. Session Name: {SessionName}", _settings.ServerId, _session.SessionName);

            _subscriptionManager = new SubscriptionManager(_session, _logger);
            _subscriptionManager.OnDataReceived += HandleDataReceived;

            if (_settings.Subscriptions != null)
            {
                foreach (var subSettings in _settings.Subscriptions)
                {
                    _subscriptionManager.CreateSubscription(subSettings);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to OPC UA server {ServerId}", settings.ServerId);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        if (_session == null)
        {
            return Task.CompletedTask;
        }

        _logger.LogInformation("Disconnecting from OPC UA server {ServerId}", _settings?.ServerId);
        _subscriptionManager?.DeleteAllSubscriptions();

        _session.Close();
        _session.Dispose();
        _session = null;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<DataPoint>> ReadAsync(IEnumerable<string> tagIds, CancellationToken cancellationToken)
    {
        if (_session is null || !_session.Connected)
        {
            throw new InvalidOperationException("Session is not connected.");
        }

        var readValueIds = tagIds.Select(id => new ReadValueId
        {
            NodeId = new NodeId(id),
            AttributeId = Attributes.Value
        }).ToList();

        _session.Read(
            null,
            0,
            TimestampsToReturn.Both,
            new ReadValueIdCollection(readValueIds),
            out DataValueCollection results,
            out DiagnosticInfoCollection diagnosticInfos);

        ClientBase.ValidateResponse(results, readValueIds);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, readValueIds);
        
        var dataPoints = new List<DataPoint>();
        for (int i = 0; i < results.Count; i++)
        {
            dataPoints.Add(new DataPoint(
                tagIds.ElementAt(i),
                results[i].Value,
                results[i].SourceTimestamp,
                results[i].StatusCode,
                DateTime.UtcNow.Ticks
            ));
        }

        return Task.FromResult<IEnumerable<DataPoint>>(dataPoints);
    }

    /// <inheritdoc/>
    public Task WriteAsync(string tagId, object value, CancellationToken cancellationToken)
    {
        if (_session is null || !_session.Connected)
        {
            throw new InvalidOperationException("Session is not connected.");
        }

        var nodesToWrite = new WriteValueCollection
        {
            new()
            {
                NodeId = new NodeId(tagId),
                AttributeId = Attributes.Value,
                Value = new DataValue(new Variant(value))
            }
        };

        _session.Write(
            null,
            nodesToWrite,
            out StatusCodeCollection results,
            out DiagnosticInfoCollection diagnosticInfos);

        ClientBase.ValidateResponse(results, nodesToWrite);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToWrite);
        
        if(StatusCode.IsBad(results[0]))
        {
            throw new ServiceResultException(results[0]);
        }

        return Task.CompletedTask;
    }

    private void HandleDataReceived(DataPoint dataPoint)
    {
        OnDataReceived?.Invoke(dataPoint);
    }
    
    private async Task<ApplicationConfiguration> CreateApplicationConfiguration()
    {
        var config = new ApplicationConfiguration()
        {
            ApplicationName = "CoreOpcClientService",
            ApplicationUri = Utils.Format(Guid.NewGuid().ToString()),
            ApplicationType = ApplicationType.Client,
            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = "Directory",
                    StorePath = "pki/own",
                    SubjectName = "CoreOpcClientService"
                },
                TrustedIssuerCertificates = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = "pki/issuer"
                },
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = "pki/trusted"
                },
                RejectedCertificateStore = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = "pki/rejected"
                },
                AutoAcceptUntrustedCertificates = true, // WARNING: For development only
                AddAppCertToTrustedStore = true
            },
            TransportConfigurations = new TransportConfigurationCollection(),
            TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
            ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
            TraceConfiguration = new TraceConfiguration()
        };

        await config.Validate(ApplicationType.Client);
        
        // Auto-accept server certificates for ease of use in dev.
        // In production, this should be false and proper certificate management should be in place.
        if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
        {
            config.CertificateValidator.CertificateValidation += (sender, e) => {
                _logger.LogWarning("Accepting untrusted certificate: {Subject}", e.Certificate.Subject);
                e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted);
            };
        }

        return config;
    }

    public async ValueTask DisposeAsync()
    {
        if (_session != null)
        {
            await DisconnectAsync(CancellationToken.None);
        }
        GC.SuppressFinalize(this);
    }
}