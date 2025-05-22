using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Confluent.Kafka;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Opc.Ua; // For Opc.Ua specific configurations
using Opc.Ua.Configuration; // For ApplicationInstance
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

        public void ConfigureGrpcClient(GrpcChannelOptions options, string serverUrl)
        {
            var useTls = _configuration.GetValue<bool>($"ServerApp:Grpc:UseTls", serverUrl.StartsWith("https://"));
            if (!useTls)
            {
                _logger.LogWarning("gRPC TLS is disabled for {ServerUrl}. Communication will be insecure.", serverUrl);
                return;
            }

            _logger.LogInformation("Configuring gRPC TLS for {ServerUrl}", serverUrl);
            var handler = new HttpClientHandler();

            var clientCertPath = _configuration["ServerApp:Grpc:ClientCertificatePath"];
            var clientCertPassword = _configuration["ServerApp:Grpc:ClientCertificatePassword"]; // If PFX has password

            if (!string.IsNullOrEmpty(clientCertPath))
            {
                try
                {
                    var clientCert = new X509Certificate2(clientCertPath, clientCertPassword);
                    handler.ClientCertificates.Add(clientCert);
                    _logger.LogInformation("Client certificate loaded for gRPC: {Subject}", clientCert.Subject);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load gRPC client certificate from {Path}", clientCertPath);
                }
            }

            var validateServerCert = _configuration.GetValue<bool>("ServerApp:Grpc:ValidateServerCertificate", true);
            if (!validateServerCert)
            {
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                _logger.LogWarning("gRPC server certificate validation is DISABLED. This is insecure and for development only.");
            }
            else
            {
                var caCertPath = _configuration["ServerApp:Grpc:CaCertificatePath"];
                if (!string.IsNullOrEmpty(caCertPath) && File.Exists(caCertPath))
                {
                     handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                     {
                         if (sslPolicyErrors == SslPolicyErrors.None) return true;

                         try
                         {
                             X509Chain customChain = new X509Chain();
                             customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                             customChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                             X509Certificate2 rootCa = new X509Certificate2(caCertPath);
                             customChain.ChainPolicy.ExtraStore.Add(rootCa);
                             
                             if (cert != null && customChain.Build(new X509Certificate2(cert)))
                             {
                                 // Check if the chain's root is our trusted CA
                                 var chainRoot = customChain.ChainElements[customChain.ChainElements.Count -1].Certificate;
                                 if (chainRoot.Thumbprint == rootCa.Thumbprint)
                                 {
                                    _logger.LogInformation("Server certificate validated against custom CA: {Subject}", cert.Subject);
                                    return true;
                                 }
                             }
                             _logger.LogWarning("Server certificate {Subject} validation failed against custom CA. Errors: {SslPolicyErrors}", cert?.Subject, sslPolicyErrors);
                             return false;
                         }
                         catch (Exception ex)
                         {
                             _logger.LogError(ex, "Exception during custom server certificate validation for gRPC.");
                             return false;
                         }
                     };
                    _logger.LogInformation("Custom CA certificate for gRPC server validation loaded from {Path}", caCertPath);
                }
            }
            options.HttpHandler = handler;
        }

        public void ConfigureRabbitMqConnectionFactory(ConnectionFactory factory, string connectionName)
        {
            var useTls = _configuration.GetValue<bool>($"ServerApp:MessageQueue:RabbitMQ:{connectionName}:UseTls", factory.Uri?.Scheme.Equals("amqps", StringComparison.OrdinalIgnoreCase) ?? false);
            if (!useTls)
            {
                 _logger.LogWarning("RabbitMQ TLS is disabled for connection {ConnectionName}. Communication will be insecure.", connectionName);
                return;
            }
            
            _logger.LogInformation("Configuring RabbitMQ TLS for connection {ConnectionName}", connectionName);
            factory.Ssl.Enabled = true;
            factory.Ssl.ServerName = factory.Uri.Host; // Use the hostname from the URI for SNI
            factory.Ssl.Version = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;

            var clientCertPath = _configuration[$"ServerApp:MessageQueue:RabbitMQ:{connectionName}:ClientCertificatePath"];
            var clientCertPass = _configuration[$"ServerApp:MessageQueue:RabbitMQ:{connectionName}:ClientCertificatePassword"];
            if (!string.IsNullOrEmpty(clientCertPath))
            {
                factory.Ssl.CertPath = clientCertPath;
                factory.Ssl.CertPassphrase = clientCertPass;
                 _logger.LogInformation("Client certificate configured for RabbitMQ connection {ConnectionName}", connectionName);
            }

            var validateServerCert = _configuration.GetValue<bool>($"ServerApp:MessageQueue:RabbitMQ:{connectionName}:ValidateServerCertificate", true);
            if (!validateServerCert)
            {
                factory.Ssl.AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable;
                _logger.LogWarning("RabbitMQ server certificate validation is DISABLED for {ConnectionName}. This is insecure.", connectionName);
            }
            else
            {
                // For custom CA, RabbitMQ.Client SslOption usually relies on the OS trust store.
                // If specific CA pinning is needed, it's more complex and might involve custom validation callbacks if supported,
                // or ensuring the CA is in the machine's trusted CA store.
                 var caCertPath = _configuration[$"ServerApp:MessageQueue:RabbitMQ:{connectionName}:CaCertificatePath"];
                 if (!string.IsNullOrEmpty(caCertPath))
                 {
                     // factory.Ssl.CaCertPath = caCertPath; // Not a direct property, typically handled by OS trust or explicit server validation hook
                     _logger.LogInformation("For RabbitMQ custom CA validation using {CaCertPath}, ensure the CA certificate is in the system's trust store or handle validation explicitly if the library allows.", caCertPath);
                 }
            }
        }

        public void ConfigureKafkaProducer(ProducerConfig config, string connectionName)
        {
            var securityProtocol = _configuration[$"ServerApp:MessageQueue:Kafka:{connectionName}:SecurityProtocol"]; // e.g., "Ssl", "SaslSsl"
            if (string.IsNullOrEmpty(securityProtocol) || securityProtocol.ToLower() == "plaintext")
            {
                _logger.LogWarning("Kafka security protocol is not SSL/TLS for {ConnectionName}. Communication may be insecure.", connectionName);
                return;
            }

            _logger.LogInformation("Configuring Kafka TLS for connection {ConnectionName} with protocol {SecurityProtocol}", connectionName, securityProtocol);
            config.SecurityProtocol = (SecurityProtocol)Enum.Parse(typeof(SecurityProtocol), securityProtocol, true);

            config.SslCaLocation = _configuration[$"ServerApp:MessageQueue:Kafka:{connectionName}:SslCaLocation"];
            config.SslCertificateLocation = _configuration[$"ServerApp:MessageQueue:Kafka:{connectionName}:SslCertificateLocation"];
            config.SslKeyLocation = _configuration[$"ServerApp:MessageQueue:Kafka:{connectionName}:SslKeyLocation"];
            config.SslKeyPassword = _configuration[$"ServerApp:MessageQueue:Kafka:{connectionName}:SslKeyPassword"]; // For PKCS#12 or encrypted PEM keys

            if (!string.IsNullOrEmpty(config.SslCaLocation))
                _logger.LogInformation("Kafka SslCaLocation set for {ConnectionName}", connectionName);
            if (!string.IsNullOrEmpty(config.SslCertificateLocation))
                _logger.LogInformation("Kafka SslCertificateLocation (client cert) set for {ConnectionName}", connectionName);

            // SASL configuration if protocol is SaslSsl
            if (securityProtocol.ToLower().Contains("sasl"))
            {
                config.SaslMechanism = (SaslMechanism)Enum.Parse(typeof(SaslMechanism), _configuration[$"ServerApp:MessageQueue:Kafka:{connectionName}:SaslMechanism"], true); // e.g., Plain, ScramSha256, ScramSha512
                config.SaslUsername = _configuration[$"ServerApp:MessageQueue:Kafka:{connectionName}:SaslUsername"];
                config.SaslPassword = _configuration[$"ServerApp:MessageQueue:Kafka:{connectionName}:SaslPassword"];
                _logger.LogInformation("Kafka SASL configured for {ConnectionName} with mechanism {SaslMechanism}", connectionName, config.SaslMechanism);
            }
        }

        public async Task ConfigureOpcUaApplicationInstance(ApplicationInstance applicationInstance, ServerConnectionConfigDto? opcUaServerConfig = null)
        {
            var config = applicationInstance.ApplicationConfiguration;

            // Load application configuration from embedded resource or file
            // This creates a default PFX if one doesn't exist.
            var pkiRootDir = _configuration.GetValue<string>("OpcClient:PkiPath", "%LocalApplicationData%/IndustrialAutomation.OpcClient/PKI");
            pkiRootDir = Environment.ExpandEnvironmentVariables(pkiRootDir);
            Directory.CreateDirectory(pkiRootDir); // Ensure PKI root directory exists

            config.SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = Path.Combine(pkiRootDir, "own"),
                    SubjectName = $"CN={config.ApplicationName}, C=US, S=Arizona, O=YourCompany, DC={Utils.GetHostName()}" // Example subject name
                },
                TrustedIssuerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = Path.Combine(pkiRootDir, "issuers")
                },
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = Path.Combine(pkiRootDir, "trusted")
                },
                RejectedCertificateStore = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = Path.Combine(pkiRootDir, "rejected")
                },
                AutoAcceptUntrustedCertificates = opcUaServerConfig?.AutoAcceptUntrustedCertificates ?? _configuration.GetValue<bool>("OpcClient:AutoAcceptUntrustedCertificates", false),
                RejectSHA1SignedCertificates = true, // Good practice
                MinimumCertificateKeySize = 2048, // Good practice
                AddAppCertToTrustedStore = true // Automatically trust its own certificate
            };
             _logger.LogInformation("OPC UA PKI Root directory set to: {PkiPath}", pkiRootDir);
             _logger.LogInformation("OPC UA ApplicationCertificate StorePath: {StorePath}", config.SecurityConfiguration.ApplicationCertificate.StorePath);
             _logger.LogInformation("OPC UA TrustedPeerCertificates StorePath: {StorePath}", config.SecurityConfiguration.TrustedPeerCertificates.StorePath);


            if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                _logger.LogWarning("OPC UA AutoAcceptUntrustedCertificates is ENABLED. This is insecure and recommended only for development/testing.");
            }
            
            applicationInstance.ApplicationConfiguration.CertificateValidator.CertificateValidation += (validator, e) =>
            {
                if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
                {
                    e.Accept = config.SecurityConfiguration.AutoAcceptUntrustedCertificates;
                    if (e.Accept)
                    {
                        _logger.LogWarning("Accepted untrusted OPC UA certificate: {SubjectName} (Thumbprint: {Thumbprint}) due to AutoAcceptUntrustedCertificates setting.", e.Certificate.SubjectName, e.Certificate.Thumbprint);
                    }
                    else
                    {
                        _logger.LogError("Rejected untrusted OPC UA certificate: {SubjectName} (Thumbprint: {Thumbprint}). Error: {Error}", e.Certificate.SubjectName, e.Certificate.Thumbprint, e.Error);
                    }
                }
                else if (e.Error != ServiceResult.Good)
                {
                     _logger.LogError("OPC UA Certificate validation error for {SubjectName}: {Error}", e.Certificate.SubjectName, e.Error);
                }
            };

            // Check the application certificate.
            bool certOk = await applicationInstance.CheckApplicationInstanceCertificate(false, CertificateFactory.DefaultKeySize).ConfigureAwait(false);
            if (!certOk)
            {
                _logger.LogError("OPC UA Application instance certificate is invalid!");
                throw new Exception("OPC UA Application instance certificate is invalid!");
            }
             _logger.LogInformation("OPC UA Application instance certificate validated successfully: {SubjectName}", config.ApplicationCertificate.SubjectName);
        }
    }
}