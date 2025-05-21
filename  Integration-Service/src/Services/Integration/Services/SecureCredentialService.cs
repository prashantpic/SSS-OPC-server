namespace IntegrationService.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using IntegrationService.Interfaces;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    // using Azure.Identity; // Example for Azure Key Vault
    // using Azure.Security.KeyVault.Secrets; // Example for Azure Key Vault

    public class SecureCredentialService : ICredentialManager
    {
        private readonly ILogger<SecureCredentialService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, (string Credential, DateTime Expiry)> _cache =
            new ConcurrentDictionary<string, (string Credential, DateTime Expiry)>();
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30); // Default cache duration

        // Example: For Azure Key Vault
        // private readonly SecretClient _secretClient;

        public SecureCredentialService(ILogger<SecureCredentialService> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Example: Initialize Azure Key Vault client
            // var keyVaultUri = _configuration["KeyVault:Uri"];
            // if (!string.IsNullOrEmpty(keyVaultUri))
            // {
            //     _secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
            //     _logger.LogInformation("SecureCredentialService initialized with Azure Key Vault: {KeyVaultUri}", keyVaultUri);
            // }
            // else
            // {
            //     _logger.LogWarning("Azure Key Vault URI not configured. SecureCredentialService will rely on environment variables or direct configuration.");
            // }

            var configuredCacheMinutes = _configuration.GetValue<int?>("Security:CredentialCacheDurationMinutes");
            if (configuredCacheMinutes.HasValue && configuredCacheMinutes.Value > 0)
            {
                _cacheDuration = TimeSpan.FromMinutes(configuredCacheMinutes.Value);
            }
        }

        public async Task<string?> GetCredentialAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                _logger.LogWarning("Credential identifier is null or empty.");
                return null;
            }

            if (_cache.TryGetValue(identifier, out var cachedEntry) && cachedEntry.Expiry > DateTime.UtcNow)
            {
                _logger.LogDebug("Returning cached credential for identifier: {Identifier}", identifier);
                return cachedEntry.Credential;
            }

            string? credential = null;

            // Priority:
            // 1. Dedicated secure store (e.g., Azure Key Vault, HashiCorp Vault) - Placeholder for now
            // 2. Environment Variables
            // 3. Direct Configuration (appsettings.json - for local dev ONLY, NOT for production secrets)

            // Placeholder for Azure Key Vault or other vault integration:
            // if (_secretClient != null)
            // {
            //     try
            //     {
            //         KeyVaultSecret secret = await _secretClient.GetSecretAsync(identifier, cancellationToken: cancellationToken);
            //         credential = secret.Value;
            //         _logger.LogInformation("Retrieved credential for identifier '{Identifier}' from Key Vault.", identifier);
            //     }
            //     catch (Exception ex) // Catch specific Azure.RequestFailedException if needed
            //     {
            //         _logger.LogWarning(ex, "Failed to retrieve credential for identifier '{Identifier}' from Key Vault. Falling back...", identifier);
            //     }
            // }

            // Fallback to Environment Variables
            if (string.IsNullOrEmpty(credential))
            {
                credential = Environment.GetEnvironmentVariable(identifier);
                if (!string.IsNullOrEmpty(credential))
                {
                    _logger.LogInformation("Retrieved credential for identifier '{Identifier}' from environment variable.", identifier);
                }
            }

            // Fallback to IConfiguration (e.g., appsettings.json - for local development ONLY)
            // Secrets should not be stored directly in appsettings.json in production.
            // This path could be for non-sensitive config or dev overrides.
            // A common pattern is "Secrets:MySecretName" or "ConnectionStrings:MyDb"
            if (string.IsNullOrEmpty(credential))
            {
                credential = _configuration[identifier]; // Direct key lookup
                if (string.IsNullOrEmpty(credential))
                {
                     // Try common prefix for secrets if not found directly
                    credential = _configuration[$"Secrets:{identifier}"];
                }

                if (!string.IsNullOrEmpty(credential))
                {
                    _logger.LogWarning("Retrieved credential for identifier '{Identifier}' from IConfiguration. This is suitable for local development but NOT for production secrets.", identifier);
                }
            }


            if (!string.IsNullOrEmpty(credential))
            {
                _cache[identifier] = (credential, DateTime.UtcNow.Add(_cacheDuration));
                _logger.LogDebug("Cached credential for identifier: {Identifier}", identifier);
                return credential;
            }

            _logger.LogError("Credential not found for identifier: {Identifier}", identifier);
            return null;
        }
    }
}