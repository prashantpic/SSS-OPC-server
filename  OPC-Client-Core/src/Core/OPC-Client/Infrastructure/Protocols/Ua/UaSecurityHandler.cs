namespace OPC.Client.Core.Infrastructure.Protocols.Ua
{
    using Microsoft.Extensions.Logging;
    using Opc.Ua;
    using Opc.Ua.Client;
    using Opc.Ua.Configuration;
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using OPC.Client.Core.Domain.ValueObjects;
    using OPC.Client.Core.Exceptions;
    using OPC.Client.Core.Utils; // For CertificateLoaderUtil

    /// <summary>
    /// Manages OPC UA security aspects like certificate validation, encryption, user authentication tokens,
    /// and secure channel configuration for OpcUaCommunicator.
    /// Implements REQ-3-001.
    /// </summary>
    public class UaSecurityHandler
    {
        private readonly ILogger<UaSecurityHandler> _logger;
        private UaSecurityConfiguration? _securityConfiguration;
        private ApplicationConfiguration? _appConfiguration; // Set by OpcUaCommunicator


        public UaSecurityHandler(ILogger<UaSecurityHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Configures the security handler with specific UA security settings and the application configuration.
        /// </summary>
        /// <param name="uaSecurityConfig">The domain value object for UA security settings.</param>
        /// <param name="appConfig">The OPC UA application configuration.</param>
        public void Configure(UaSecurityConfiguration uaSecurityConfig, ApplicationConfiguration appConfig)
        {
            _securityConfiguration = uaSecurityConfig ?? throw new ArgumentNullException(nameof(uaSecurityConfig));
            _appConfiguration = appConfig ?? throw new ArgumentNullException(nameof(appConfig));

            _logger.LogInformation("UaSecurityHandler configured. Policy: {Policy}, Mode: {Mode}, AutoAccept: {AutoAccept}",
                _securityConfiguration.SecurityPolicyUri ?? "BestAvailable",
                _securityConfiguration.MessageSecurityMode,
                _securityConfiguration.AutoAcceptUntrustedCertificates);

            // Apply domain security configuration to the SDK's ApplicationConfiguration
            ApplySecuritySettingsToAppConfig();
        }

        /// <summary>
        /// Applies the domain UaSecurityConfiguration settings to the SDK's ApplicationConfiguration.
        /// This prepares the ApplicationConfiguration for session creation.
        /// </summary>
        private void ApplySecuritySettingsToAppConfig()
        {
            if (_appConfiguration == null || _securityConfiguration == null)
            {
                _logger.LogError("Cannot apply security settings: ApplicationConfiguration or UaSecurityConfiguration is null.");
                return;
            }

            var sdkSecurityConfig = _appConfiguration.SecurityConfiguration ?? new SecurityConfiguration();

            // Client Application Certificate
            if (!string.IsNullOrWhiteSpace(_securityConfiguration.ClientCertificateFilePath))
            {
                sdkSecurityConfig.ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = CertificateStoreType.Directory, // SDK uses Directory for file paths
                    StorePath = System.IO.Path.GetDirectoryName(_securityConfiguration.ClientCertificateFilePath), // SDK expects directory
                    SubjectName = System.IO.Path.GetFileName(_securityConfiguration.ClientCertificateFilePath) // SDK uses SubjectName for file name
                };
                // Password for PFX is handled by CertificateLoaderUtil when finding/loading if SDK doesn't use it directly here
                _logger.LogInformation("Application certificate configured from file: {FilePath}", _securityConfiguration.ClientCertificateFilePath);
            }
            else if (!string.IsNullOrWhiteSpace(_securityConfiguration.ClientCertificateStorePath) &&
                     (!string.IsNullOrWhiteSpace(_securityConfiguration.ClientCertificateThumbprint) ||
                      !string.IsNullOrWhiteSpace(_securityConfiguration.ClientCertificateSubjectName)))
            {
                sdkSecurityConfig.ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = CertificateStoreType.X509Store,
                    StorePath = _securityConfiguration.ClientCertificateStorePath,
                    SubjectName = _securityConfiguration.ClientCertificateThumbprint ?? _securityConfiguration.ClientCertificateSubjectName
                    // SDK will search by subject name or thumbprint within this store
                };
                _logger.LogInformation("Application certificate configured from store: {StorePath}, Identifier: {Identifier}",
                    _securityConfiguration.ClientCertificateStorePath,
                    _securityConfiguration.ClientCertificateThumbprint ?? _securityConfiguration.ClientCertificateSubjectName);
            }
            else
            {
                _logger.LogWarning("No client application certificate configured. Secure connection might fail if required by server.");
                sdkSecurityConfig.ApplicationCertificate = new CertificateIdentifier(); // Ensure it's at least initialized
            }


            // Trusted Peer (Server) Certificates Store
            sdkSecurityConfig.TrustedPeerCertificates = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory, // Default, or parse from path
                StorePath = _securityConfiguration.TrustedPeerCertificateStorePath ?? "pki/trusted" // SDK default or from config
            };
             _logger.LogInformation("TrustedPeerCertificates store configured: {StorePath}", sdkSecurityConfig.TrustedPeerCertificates.StorePath);


            // Rejected Peer Certificates Store
            sdkSecurityConfig.RejectedCertificateStore = new CertificateStoreIdentifier
            {
                StoreType = CertificateStoreType.Directory, // Default
                StorePath = _securityConfiguration.RejectedCertificateStorePath ?? "pki/rejected" // SDK default or from config
            };
             _logger.LogInformation("RejectedCertificateStore configured: {StorePath}", sdkSecurityConfig.RejectedCertificateStore.StorePath);

            // Issuer (CA) Certificates Store
            sdkSecurityConfig.TrustedIssuerCertificates = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory, // Default
                StorePath = _securityConfiguration.IssuerCertificateStorePath ?? "pki/issuers" // SDK default or from config
            };
             _logger.LogInformation("TrustedIssuerCertificates store configured: {StorePath}", sdkSecurityConfig.TrustedIssuerCertificates.StorePath);


            // Auto-accept untrusted certificates (use with extreme caution)
            sdkSecurityConfig.AutoAcceptUntrustedCertificates = _securityConfiguration.AutoAcceptUntrustedCertificates;
            if (sdkSecurityConfig.AutoAcceptUntrustedCertificates)
            {
                _logger.LogWarning("AutoAcceptUntrustedCertificates is ENABLED. This is a security risk in production environments.");
            }

            _appConfiguration.SecurityConfiguration = sdkSecurityConfig;

            // The ApplicationConfiguration should be validated after these changes, typically by the OpcUaCommunicator.
        }


        /// <summary>
        /// Selects an appropriate endpoint from the server based on the configured security policy and mode.
        /// </summary>
        /// <param name="endpointUrl">The discovery URL of the OPC UA server.</param>
        /// <returns>The selected EndpointDescription.</returns>
        /// <exception cref="OpcCommunicationException">If no suitable endpoint is found.</exception>
        public async Task<EndpointDescription> SelectEndpointAsync(string endpointUrl)
        {
            if (_appConfiguration == null || _securityConfiguration == null)
                throw new InvalidOperationException("UaSecurityHandler is not configured.");

            _logger.LogInformation("Selecting endpoint for URL: {EndpointUrl} with Policy: {Policy}, Mode: {Mode}",
                endpointUrl, _securityConfiguration.SecurityPolicyUri ?? "BestAvailable", _securityConfiguration.MessageSecurityMode);

            try
            {
                var uri = new Uri(endpointUrl);
                var client = DiscoveryClient.Create(uri, _appConfiguration); // Pass appConfig for discovery client settings
                var endpoints = await client.GetEndpointsAsync(null); // Pass null for RequestHeader
                client.Close(); // Close discovery client

                if (endpoints == null || endpoints.Count == 0)
                {
                    _logger.LogError("No endpoints found at {EndpointUrl}", endpointUrl);
                    throw new OpcCommunicationException($"No endpoints found at {endpointUrl}");
                }

                EndpointDescription? selectedEndpoint = null;

                // Filter by security policy URI if specified
                var policyFilteredEndpoints = string.IsNullOrWhiteSpace(_securityConfiguration.SecurityPolicyUri)
                    ? endpoints.ToArray()
                    : endpoints.Where(e => e.SecurityPolicyUri == _securityConfiguration.SecurityPolicyUri).ToArray();

                if (!policyFilteredEndpoints.Any())
                {
                    _logger.LogError("No endpoints found matching security policy: {PolicyUri}", _securityConfiguration.SecurityPolicyUri);
                    throw new OpcCommunicationException($"No endpoints found matching security policy: {_securityConfiguration.SecurityPolicyUri}");
                }

                // Select based on MessageSecurityMode
                foreach (var ep in policyFilteredEndpoints.OrderByDescending(e => e.SecurityLevel)) // Prefer higher security level
                {
                    var mode = (MessageSecurityMode)Enum.Parse(typeof(MessageSecurityMode), _securityConfiguration.MessageSecurityMode, true);
                    if (ep.SecurityMode == mode)
                    {
                        selectedEndpoint = ep;
                        break;
                    }
                }

                // Fallback: if exact mode not found, try to pick best available if mode is not "None"
                if (selectedEndpoint == null && _securityConfiguration.MessageSecurityMode != "None")
                {
                    selectedEndpoint = policyFilteredEndpoints
                        .Where(e => e.SecurityMode != MessageSecurityMode.None) // Exclude "None" if we want security
                        .OrderByDescending(e => e.SecurityLevel)
                        .FirstOrDefault();
                }
                else if (selectedEndpoint == null && _securityConfiguration.MessageSecurityMode == "None") // If "None" is configured
                {
                     selectedEndpoint = policyFilteredEndpoints
                        .FirstOrDefault(e => e.SecurityMode == MessageSecurityMode.None);
                }


                if (selectedEndpoint == null)
                {
                    _logger.LogError("No suitable endpoint found with Policy: {Policy} and Mode: {Mode}",
                        _securityConfiguration.SecurityPolicyUri ?? "Any", _securityConfiguration.MessageSecurityMode);
                    throw new OpcCommunicationException("No suitable endpoint found matching security configuration.");
                }

                _logger.LogInformation("Selected endpoint: {SelectedEndpointUrl}, Policy: {SelectedPolicy}, Mode: {SelectedMode}, Level: {SecurityLevel}",
                    selectedEndpoint.EndpointUrl, selectedEndpoint.SecurityPolicyUri, selectedEndpoint.SecurityMode, selectedEndpoint.SecurityLevel);

                return selectedEndpoint;
            }
            catch (ServiceResultException sre)
            {
                _logger.LogError(sre, "ServiceResultException during endpoint selection for {EndpointUrl}: {SreMessage}", endpointUrl, sre.Message);
                throw new OpcCommunicationException($"Endpoint selection failed for {endpointUrl}: {sre.Message}", sre);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during endpoint selection for {EndpointUrl}", endpointUrl);
                throw new OpcCommunicationException($"Endpoint selection failed for {endpointUrl}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a UserIdentity object for session authentication based on the configured UaUserIdentity.
        /// </summary>
        /// <param name="userIdentityConfig">The UaUserIdentity domain object.</param>
        /// <returns>An Opc.Ua.UserIdentity object.</returns>
        /// <exception cref="OpcSecurityException">If creating user identity fails (e.g., certificate not found).</exception>
        public UserIdentity GetUserIdentityToken(UaUserIdentity userIdentityConfig)
        {
            _logger.LogInformation("Creating UserIdentity token for type: {IdentityType}", userIdentityConfig.Type);
            try
            {
                switch (userIdentityConfig.Type)
                {
                    case UserIdentityType.Anonymous:
                        return new UserIdentity(); // AnonymousIdentityToken is default

                    case UserIdentityType.UserName:
                        return new UserIdentity(userIdentityConfig.Username, userIdentityConfig.Password);

                    case UserIdentityType.Certificate:
                        X509Certificate2? userCert = null;
                        if (!string.IsNullOrWhiteSpace(userIdentityConfig.CertificateFilePath))
                        {
                            userCert = CertificateLoaderUtil.LoadCertificateFromFile(
                                userIdentityConfig.CertificateFilePath,
                                userIdentityConfig.CertificateFilePassword);
                        }
                        else if (!string.IsNullOrWhiteSpace(userIdentityConfig.CertificateStorePath) &&
                                 !string.IsNullOrWhiteSpace(userIdentityConfig.CertificateThumbprint))
                        {
                            userCert = CertificateLoaderUtil.LoadCertificateFromStore(
                                userIdentityConfig.CertificateStorePath,
                                X509FindType.FindByThumbprint,
                                userIdentityConfig.CertificateThumbprint);
                        }
                        else
                        {
                            throw new OpcSecurityException("Certificate user identity requires either file path or store path and thumbprint.");
                        }

                        if (userCert == null)
                            throw new OpcSecurityException($"User certificate not found for identity type {userIdentityConfig.Type}.");
                        return new UserIdentity(userCert);

                    case UserIdentityType.IssuedToken:
                        // IssuedToken type can be complex. This is a basic representation.
                        // The token type and policy ID might be required by the server.
                        IssuedIdentityToken issuedToken = new IssuedIdentityToken
                        {
                            TokenData = Convert.FromBase64String(userIdentityConfig.IssuedToken ?? ""), // Assuming token is base64
                            EncryptionAlgorithm = null, // Depends on server policy
                            PolicyId = userIdentityConfig.PolicyId
                        };
                        return new UserIdentity(issuedToken);

                    default:
                        throw new NotSupportedException($"User identity type {userIdentityConfig.Type} is not supported.");
                }
            }
            catch (Exception ex) when (ex is not OpcSecurityException && ex is not NotSupportedException)
            {
                _logger.LogError(ex, "Failed to create user identity token for type {IdentityType}", userIdentityConfig.Type);
                throw new OpcSecurityException($"Failed to create user identity token: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Callback for server certificate validation.
        /// </summary>
        public void OnCertificateValidation(object? sender, CertificateValidationEventArgs e)
        {
            if (e.Certificate.RawData == null)
            {
                 _logger.LogError("Server certificate validation failed: Certificate data is null.");
                 e.Accept = false;
                 return;
            }
            _logger.LogInformation("Validating server certificate: Subject='{Subject}', Thumbprint='{Thumbprint}', Issuer='{Issuer}'",
                e.Certificate.Subject, e.Certificate.Thumbprint, e.Certificate.Issuer);

            if (e.Error != null && StatusCode.IsGood(e.Error.StatusCode))
            {
                _logger.LogInformation("Server certificate validation passed basic SDK checks.");
                e.Accept = true;
                return;
            }

            // If SDK validation failed (e.g., untrusted root, chain error)
            _logger.LogWarning("Server certificate validation failed SDK checks: {StatusCode} - {StatusInfo}", e.Error?.StatusCode, e.Error?.AdditionalInfo);

            if (_securityConfiguration?.AutoAcceptUntrustedCertificates == true)
            {
                _logger.LogWarning("AutoAcceptUntrustedCertificates is TRUE. Accepting server certificate despite validation errors. This is a SECURITY RISK.");
                e.Accept = true;
                return;
            }

            _logger.LogError("Server certificate rejected due to validation errors and AutoAcceptUntrustedCertificates is FALSE.");
            e.Accept = false;

            // Optionally, add to rejected store if configured
            if (_appConfiguration?.SecurityConfiguration?.RejectedCertificateStore?.StorePath != null)
            {
                try
                {
                    var rejectedStore = CertificateStoreIdentifier.Open(_appConfiguration.SecurityConfiguration.RejectedCertificateStore);
                    rejectedStore.Add(e.Certificate).GetAwaiter().GetResult();
                    _logger.LogInformation("Rejected server certificate added to rejected store: {StorePath}", rejectedStore.Path);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to add rejected server certificate to store.");
                }
            }
        }
    }
}