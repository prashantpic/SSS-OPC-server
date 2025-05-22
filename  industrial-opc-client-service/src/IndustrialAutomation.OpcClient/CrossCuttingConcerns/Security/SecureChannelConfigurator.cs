using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Configuration;
using RabbitMQ.Client; // For RabbitMQ ConnectionFactory
using Confluent.Kafka; // For Kafka ProducerConfig
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using IndustrialAutomation.OpcClient.Application.DTOs.Common; // For ServerConnectionConfigDto

namespace IndustrialAutomation.OpcClient.CrossCuttingConcerns.Security
{
    public class SecureChannelConfigurator
    {
        private readonly ILogger<SecureChannelConfigurator> _logger;
        private readonly IConfiguration _configuration;

        public SecureChannelConfigurator(ILogger<SecureChannelConfigurator> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void ConfigureGrpcClient(GrpcChannelOptions options, string? clientCertPath = null, string? clientCertPassword = null, string? caCertPath = null)
        {
            _logger.LogInformation("Configuring secure gRPC client channel.");
            var handler = new HttpClientHandler();

            X509Certificate2? clientCertificate = null;
            if (!string.IsNullOrEmpty(clientCertPath))
            {
                try
                {
                    clientCertificate = new X509Certificate2(clientCertPath, clientCertPassword);
                    handler.ClientCertificates.Add(clientCertificate);
                    _logger.LogInformation("Loaded client certificate for gRPC: {Subject}", clientCertificate.Subject);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load client certificate from path: {ClientCertPath}", clientCertPath);
                }
            }

            if (!string.IsNullOrEmpty(caCertPath))
            {
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
                    if (sslPolicyErrors == SslPolicyErrors.None) return true;

                    try
                    {
                        X509Certificate2 ca = new X509Certificate2(caCertPath);
                        X509Chain customChain = new X509Chain();
                        customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                        customChain.ChainPolicy.ExtraStore.Add(ca); // Add your CA to the chain's store

                        if (cert != null)
                        {
                           customChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                           bool isValid = customChain.Build(new X509Certificate2(cert));
                           if (isValid) {
                               _logger.LogDebug("Server certificate validated successfully against custom CA for gRPC.");
                               return true;
                           }
                           else {
                                _logger.LogWarning("Server certificate validation failed against custom CA for gRPC. Chain status: {StatusInfo}", customChain.ChainStatus.FirstOrDefault().StatusInformation);
                           }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during custom CA validation for gRPC server certificate.");
                    }
                    _logger.LogWarning("gRPC server certificate validation failed with SslPolicyErrors: {SslPolicyErrors}", sslPolicyErrors);
                    return false; // Fail validation if custom CA check fails or errors occur
                };
            }
            else
            {
                // Default behavior if no custom CA is provided: rely on system trust store.
                // Log if server certificate validation fails.
                 handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
                    if (sslPolicyErrors == SslPolicyErrors.None) return true;
                    _logger.LogWarning("gRPC server certificate validation failed (using system CA). SslPolicyErrors: {SslPolicyErrors}", sslPolicyErrors);
                    if(cert != null) _logger.LogWarning("Server Cert Subject: {Subject}, Issuer: {Issuer}", cert.Subject, cert.Issuer);
                    return false; // Or true if you want to allow self-signed for dev. BE CAREFUL.
                };
            }

            options.HttpHandler = handler;
        }

        public void ConfigureRabbitMqConnectionFactory(ConnectionFactory factory, string? clientCertPath = null, string? clientCertPassword = null, string? caCertPath = null, string? serverNameOverride = null)
        {
            _logger.LogInformation("Configuring secure RabbitMQ connection factory.");
            if (factory.Uri != null && factory.Uri.Scheme.Equals("amqps", StringComparison.OrdinalIgnoreCase))
            {
                factory.Ssl.Enabled = true;
                factory.Ssl.Version = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
                factory.Ssl.AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch; // Allow if CN doesn't match hostname but cert is trusted

                if (!string.IsNullOrEmpty(serverNameOverride)) {
                    factory.Ssl.ServerName = serverNameOverride; // SNI
                }


                if (!string.IsNullOrEmpty(clientCertPath))
                {
                    factory.Ssl.CertPath = clientCertPath;
                    if (!string.IsNullOrEmpty(clientCertPassword)) // Some SDKs might need password via different means
                    {
                        factory.Ssl.CertPassphrase = clientCertPassword;
                    }
                    _logger.LogInformation("Configured client certificate for RabbitMQ from path: {ClientCertPath}", clientCertPath);
                }

                if (!string.IsNullOrEmpty(caCertPath))
                {
                    factory.Ssl.CaCertPath = caCertPath; // Path to CA bundle
                     _logger.LogInformation("Configured CA certificate for RabbitMQ from path: {CaCertPath}", caCertPath);
                }
                // Depending on RabbitMQ server config, might need to set Ssl.ServerName for SNI
            }
        }

        public void ConfigureKafkaProducer(ClientConfig config, string? clientCertPath = null, string? clientKeyPath = null, string? caCertPath = null)
        {
             _logger.LogInformation("Configuring secure Kafka client.");
            // Assumes broker address in config includes port and implies SSL if necessary, e.g., "myserver.com:9093"
            // For Kafka, security protocol is often set directly, e.g., SecurityProtocol.Ssl
            if (config.SecurityProtocol == SecurityProtocol.Ssl || 
                config.SecurityProtocol == SecurityProtocol.SaslSsl)
            {
                 _logger.LogInformation("Kafka security protocol set to SSL/SASL_SSL.");
                if (!string.IsNullOrEmpty(caCertPath))
                {
                    config.SslCaLocation = caCertPath;
                     _logger.LogInformation("Configured Kafka SSL CA location: {SslCaLocation}", caCertPath);
                }
                if (!string.IsNullOrEmpty(clientCertPath))
                {
                    config.SslCertificateLocation = clientCertPath;
                    _logger.LogInformation("Configured Kafka SSL certificate location: {SslCertificateLocation}", clientCertPath);

                }
                 if (!string.IsNullOrEmpty(clientKeyPath))
                {
                    config.SslKeyLocation = clientKeyPath; // Client private key
                    // config.SslKeyPassword = clientKeyPassword; // If key is password protected
                     _logger.LogInformation("Configured Kafka SSL key location: {SslKeyLocation}", clientKeyPath);
                }
                // Other SSL settings: SslCipherSuites, SslEndpointIdentificationAlgorithm, etc.
                // config.SslEndpointIdentificationAlgorithm = "https"; // Enable hostname verification
            }
        }

        public async Task<ApplicationConfiguration> ConfigureOpcUaApplicationAsync(ServerConnectionConfigDto connectionConfig, string applicationName = "IndustrialOpcClient")
        {
            _logger.LogInformation("Configuring OPC UA application: {ApplicationName}", applicationName);

            var config = new ApplicationConfiguration()
            {
                ApplicationName = applicationName,
                ApplicationUri = Utils.Format(@"urn:{0}:{1}", System.Net.Dns.GetHostName(), applicationName),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier(), // Filled below
                    TrustedIssuerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities\certs" },
                    TrustedPeerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications\certs" },
                    RejectedCertificateStore = new CertificateTrustList { StoreType = "Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates" },
                    AutoAcceptUntrustedCertificates = connectionConfig.AutoAcceptUntrustedCertificates, // DANGEROUS for production
                    RejectSHA1SignedCertificates = true, // Good practice
                    MinimumCertificateKeySize = 2048 // Good practice
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 120000 }, // 2 minutes
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 }, // 1 minute
                TraceConfiguration = new TraceConfiguration() // Enable tracing if needed
            };

            // Ensure store paths exist
            Directory.CreateDirectory(Path.GetFullPath(config.SecurityConfiguration.TrustedIssuerCertificates.StorePath!));
            Directory.CreateDirectory(Path.GetFullPath(config.SecurityConfiguration.TrustedPeerCertificates.StorePath!));
            Directory.CreateDirectory(Path.GetFullPath(config.SecurityConfiguration.RejectedCertificateStore.StorePath!));


            // Configure application certificate
            if (!string.IsNullOrEmpty(connectionConfig.ClientCertificatePath) && !string.IsNullOrEmpty(connectionConfig.ClientPrivateKeyPath))
            {
                 _logger.LogInformation("Attempting to load OPC UA client certificate from PFX/PEM: {ClientCertificatePath}", connectionConfig.ClientCertificatePath);
                // This is complex. OPC UA SDK usually handles creation or expects a PFX/DER in specific store.
                // If providing path to PFX:
                // config.SecurityConfiguration.ApplicationCertificate.StoreType = CertificateStoreType.X509Store;
                // config.SecurityConfiguration.ApplicationCertificate.StorePath = "CurrentUser\\My"; // Example store
                // config.SecurityConfiguration.ApplicationCertificate.SubjectName = "CN=MyClientCertName"; // Or Thumbprint
                // OR load from file and put into store, then reference by thumbprint.
                // For now, let's assume a certificate will be created or found by SDK if not explicitly configured via store/thumbprint.
                // If using a specific file:
                 config.SecurityConfiguration.ApplicationCertificate.StoreType = "X509File";
                 config.SecurityConfiguration.ApplicationCertificate.StorePath = connectionConfig.ClientCertificatePath;
                 // Password for PFX might be needed if not automatically handled by SDK or if private key is separate
                 // config.SecurityConfiguration.ApplicationCertificate.PrivateKeyPassword = connectionConfig.Password; // if PFX password
                 _logger.LogInformation("OPC UA client certificate configured from file path: {ClientCertificatePath}", connectionConfig.ClientCertificatePath);
            }
            else if (!string.IsNullOrEmpty(connectionConfig.ClientCertificateThumbprint))
            {
                 _logger.LogInformation("Using OPC UA client certificate by thumbprint: {ClientCertificateThumbprint}", connectionConfig.ClientCertificateThumbprint);
                config.SecurityConfiguration.ApplicationCertificate.StoreType = CertificateStoreType.X509Store;
                // Common stores: "CurrentUser\\My", "LocalMachine\\My"
                config.SecurityConfiguration.ApplicationCertificate.StorePath = _configuration.GetValue<string>("OpcUa:ClientCertificate:StorePath", "CurrentUser\\My");
                config.SecurityConfiguration.ApplicationCertificate.Thumbprint = connectionConfig.ClientCertificateThumbprint;
            }
            else
            {
                _logger.LogInformation("No explicit OPC UA client certificate path or thumbprint provided. SDK will attempt to find/create one.");
                // Standard SDK behavior: creates a self-signed cert if none found.
                // Default path for SDK created certs: %CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault
                config.SecurityConfiguration.ApplicationCertificate.StoreType = CertificateStoreType.Directory;
                config.SecurityConfiguration.ApplicationCertificate.StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault";
                config.SecurityConfiguration.ApplicationCertificate.SubjectName = config.ApplicationName; // SDK will create cert with this subject
            }


            await config.Validate(ApplicationType.Client);

            // Auto-accept server certificate if configured (dev only)
            if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                config.CertificateValidator.CertificateValidation += (sender, e) =>
                {
                    if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
                    {
                        _logger.LogWarning("Auto-accepting untrusted OPC UA server certificate (DEV ONLY): {SubjectName}, Error: {Error}", e.Certificate.SubjectName, e.Error);
                        e.Accept = true;
                    }
                };
            }
             else // Implement proper validation or trust mechanism
            {
                config.CertificateValidator.CertificateValidation += (sender, e) =>
                {
                    if (e.Error.StatusCode != StatusCodes.Good)
                    {
                         _logger.LogWarning("OPC UA Server Certificate Validation Error: {Error}, Subject: {SubjectName}, Thumbprint: {Thumbprint}", e.Error, e.Certificate.SubjectName.Name, e.Certificate.Thumbprint);
                    }
                    // If not auto-accepting, default behavior is to reject untrusted.
                    // Custom logic here could check against a specific CA or list of trusted thumbprints if not using standard OPC UA trust lists.
                    e.Accept = (e.Error.StatusCode == StatusCodes.Good); // Only accept if Good
                };
            }

            return config;
        }
    }
}