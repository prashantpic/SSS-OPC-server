using Opc.Client.Core.Application.Interfaces;
using Opc.Client.Core.Domain.Aggregates;
using Opc.Client.Core.Domain.ValueObjects;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace Opc.Client.Core.Infrastructure.Opc.Protocols;

/// <summary>
/// A concrete client for interacting with OPC UA servers. Implements the common IOpcProtocolClient interface.
/// </summary>
/// <remarks>
/// This class encapsulates the complexity of the OPC Foundation .NET Standard SDK for all OPC UA communications.
/// It manages session lifecycle, security, subscriptions, and data access.
/// </remarks>
public class OpcUaClient : IOpcProtocolClient, IDisposable
{
    private readonly ApplicationConfiguration _appConfig;
    private Session? _session;
    // These would be more complex implementations in a real system
    private readonly UaSubscriptionManager _subscriptionManager;
    private readonly UaSecurityHandler _securityHandler = new();

    public OpcUaClient()
    {
        // This configuration would typically be loaded from a file
        _appConfig = new ApplicationConfiguration
        {
            ApplicationName = "Opc.Client.Core",
            ApplicationUri = Utils.Format("urn:{0}:Opc.Client.Core", System.Net.Dns.GetHostName()),
            ApplicationType = ApplicationType.Client,
            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier { StoreType = "X509Store", StorePath = "CurrentUser\\My", SubjectName = "Opc.Client.Core" },
                TrustedIssuerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = "OPC Foundation/CertificateStores/UA Certificate Authorities" },
                TrustedPeerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = "OPC Foundation/CertificateStores/UA Applications" },
                RejectedCertificateStore = new CertificateTrustList { StoreType = "Directory", StorePath = "OPC Foundation/CertificateStores/RejectedCertificates" },
                AutoAcceptUntrustedCertificates = true // Should be false in production
            },
            TransportConfigurations = new TransportConfigurationCollection(),
            TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
            ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
            TraceConfiguration = new TraceConfiguration()
        };
        _appConfig.Validate(ApplicationType.Client).GetAwaiter().GetResult();
        _appConfig.CertificateValidator.CertificateValidation += _securityHandler.OnCertificateValidation;

        _subscriptionManager = new UaSubscriptionManager(() => _session);
    }

    /// <inheritdoc/>
    public async Task<ServerStatus> ConnectAsync(ServerConfiguration config, CancellationToken cancellationToken)
    {
        if (_session is { Connected: true })
        {
            if (_session.Endpoint.EndpointUrl.Equals(config.EndpointUrl, StringComparison.OrdinalIgnoreCase))
                return ServerStatus.Connected;
            await DisconnectAsync(cancellationToken);
        }

        try
        {
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(config.EndpointUrl, useSecurity: !string.IsNullOrEmpty(config.SecurityPolicy), 15000);
            var endpointConfiguration = EndpointConfiguration.Create(selectedEndpoint.Configuration);
            var configuredEndpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);
            
            var identity = new UserIdentity(config.UserName, config.Password);

            _session = await Session.Create(
                _appConfig,
                configuredEndpoint,
                updateBeforeConnect: false,
                checkDomain: false,
                _appConfig.ApplicationName,
                (uint)_appConfig.ClientConfiguration.DefaultSessionTimeout,
                identity,
                preferredLocales: null
            );

            return _session.Connected ? ServerStatus.Connected : ServerStatus.Faulted;
        }
        catch (Exception)
        {
            // Log exception
            return ServerStatus.Faulted;
        }
    }

    /// <inheritdoc/>
    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        if (_session == null) return Task.CompletedTask;
        _session.Close();
        _session.Dispose();
        _session = null;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<TagValue>> ReadAsync(IEnumerable<string> nodeIds, CancellationToken cancellationToken)
    {
        if (_session == null || !_session.Connected) throw new InvalidOperationException("Session is not connected.");

        var readValueIds = new ReadValueIdCollection(nodeIds.Select(id => new ReadValueId
        {
            NodeId = new NodeId(id),
            AttributeId = Attributes.Value
        }));

        _session.Read(null, 0, TimestampsToReturn.Both, readValueIds, out var results, out var diagnosticInfos);

        var response = results.Select((dv, i) => new TagValue(
            dv.Value,
            StatusCode.IsGood(dv.StatusCode) ? OpcQuality.Good : StatusCode.IsUncertain(dv.StatusCode) ? OpcQuality.Uncertain : OpcQuality.Bad,
            dv.SourceTimestamp
        ));

        return Task.FromResult(response);
    }

    /// <inheritdoc/>
    public Task<WriteResult> WriteAsync(IDictionary<string, object> values, CancellationToken cancellationToken)
    {
        if (_session == null || !_session.Connected) throw new InvalidOperationException("Session is not connected.");

        var writeValues = new WriteValueCollection();
        foreach (var entry in values)
        {
            writeValues.Add(new WriteValue
            {
                NodeId = new NodeId(entry.Key),
                AttributeId = Attributes.Value,
                Value = new DataValue(new Variant(entry.Value))
            });
        }

        _session.Write(null, writeValues, out var results, out var diagnosticInfos);

        var nodeStatus = values.Keys.Zip(results, (key, status) => new { key, status })
                                    .ToDictionary(x => x.key, x => (uint)x.status.Code);
        
        bool isSuccess = results.All(StatusCode.IsGood);
        string? errorMessage = isSuccess ? null : "One or more writes failed.";

        return Task.FromResult(new WriteResult(isSuccess, errorMessage, nodeStatus));
    }

    /// <inheritdoc/>
    public Task<IEnumerable<NodeInfo>> BrowseAsync(string? nodeId, CancellationToken cancellationToken)
    {
        if (_session == null || !_session.Connected) throw new InvalidOperationException("Session is not connected.");

        var nodesToBrowse = new BrowseDescriptionCollection
        {
            new() {
                NodeId = nodeId != null ? new NodeId(nodeId) : ObjectIds.RootFolder,
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
                IncludeSubtypes = true,
                NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable),
                ResultMask = (uint)BrowseResultMask.All
            }
        };

        var references = _session.Browse(null, null, 0, nodesToBrowse, out var diagnosticInfos);

        var result = references[0].References
            .Select(rd => new NodeInfo(rd.NodeId.ToString(), rd.DisplayName.Text, rd.NodeClass.ToString()))
            .ToList();

        return Task.FromResult<IEnumerable<NodeInfo>>(result);
    }

    /// <inheritdoc/>
    public Task<Guid> CreateSubscriptionAsync(SubscriptionParameters parameters, Action<DataChangeNotification> onNotification, CancellationToken cancellationToken)
    {
        if (_session == null || !_session.Connected) throw new InvalidOperationException("Session is not connected.");
        return _subscriptionManager.CreateSubscriptionAsync(parameters, onNotification, cancellationToken);
    }

    public void Dispose()
    {
        DisconnectAsync(CancellationToken.None).GetAwaiter().GetResult();
        _session?.Dispose();
        GC.SuppressFinalize(this);
    }
}

// --- Placeholder implementations for compilation ---

public class UaSecurityHandler
{
    public void OnCertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
    {
        // In a real application, this would have logic to check against a trust list
        // and potentially prompt a user or use pre-configured rules.
        if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
        {
            e.Accept = true; // Auto-accept for development, VERY INSECURE FOR PRODUCTION.
        }
    }
}

public class UaSubscriptionManager
{
    private readonly Func<Session?> _getSession;

    public UaSubscriptionManager(Func<Session?> getSession)
    {
        _getSession = getSession;
    }

    public Task<Guid> CreateSubscriptionAsync(SubscriptionParameters parameters, Action<DataChangeNotification> onNotification, CancellationToken cancellationToken)
    {
        var session = _getSession() ?? throw new InvalidOperationException("Session is not available.");

        var subscription = new Subscription(session.DefaultSubscription)
        {
            PublishingInterval = (int)parameters.PublishingInterval,
            DisplayName = $"Sub_{Guid.NewGuid()}"
        };

        var items = parameters.NodeIdsToMonitor.Select(nodeId => new MonitoredItem(subscription.DefaultItem)
        {
            StartNodeId = new NodeId(nodeId),
            AttributeId = Attributes.Value,
            DisplayName = nodeId,
            SamplingInterval = 1000 // Could be customized
        }).ToList();
        
        items.ForEach(item => item.Notification += (monitoredItem, args) =>
        {
            if (args.NotificationValue is DataValue value)
            {
                var tagValue = new TagValue(
                    value.Value,
                    StatusCode.IsGood(value.StatusCode) ? OpcQuality.Good : OpcQuality.Bad,
                    value.SourceTimestamp);
                
                onNotification(new Application.Interfaces.DataChangeNotification(monitoredItem.DisplayName, tagValue));
            }
        });
        
        subscription.AddItems(items);
        session.AddSubscription(subscription);
        subscription.Create();

        return Task.FromResult(subscription.Id != null ? new Guid(subscription.Id.ToString()) : Guid.NewGuid());
    }
}