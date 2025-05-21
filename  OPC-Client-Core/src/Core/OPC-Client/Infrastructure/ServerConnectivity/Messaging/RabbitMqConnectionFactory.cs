using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.IO;
using System.Threading;
using OPC.Client.Core.Application; // For RabbitMqConfiguration

namespace OPC.Client.Core.Infrastructure.ServerConnectivity.Messaging
{
    /// <summary>
    /// Manages RabbitMQ connection and channel lifecycle, ensuring resilient connectivity.
    /// Implements REQ-SAP-003.
    /// </summary>
    public class RabbitMqConnectionFactory : IDisposable
    {
        private readonly ILogger<RabbitMqConnectionFactory> _logger;
        private readonly RabbitMqConfiguration _config;
        private IConnection? _connection;
        private readonly object _connectionLock = new object();
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqConnectionFactory"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="rabbitMqOptions">The RabbitMQ configuration options.</param>
        public RabbitMqConnectionFactory(ILogger<RabbitMqConnectionFactory> logger, IOptions<RabbitMqConfiguration> rabbitMqOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = rabbitMqOptions?.Value ?? throw new ArgumentNullException(nameof(rabbitMqOptions), "RabbitMQ configuration is null.");
            ValidateConfiguration(_config);
        }
        
        private void ValidateConfiguration(RabbitMqConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(config.Hostname))
                throw new ArgumentException("RabbitMQ Hostname must be configured.", nameof(config.Hostname));
            if (string.IsNullOrWhiteSpace(config.Username))
                _logger.LogWarning("RabbitMQ Username is not configured; defaulting might be used by the client library.");
            if (string.IsNullOrWhiteSpace(config.Password))
                _logger.LogWarning("RabbitMQ Password is not configured; defaulting might be used by the client library.");
        }

        /// <summary>
        /// Gets a value indicating whether the RabbitMQ connection is currently open.
        /// </summary>
        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        /// <summary>
        /// Attempts to create or retrieve an existing open RabbitMQ connection.
        /// Implements connection resilience with retry logic.
        /// </summary>
        /// <returns>An open RabbitMQ connection.</returns>
        /// <exception cref="BrokerUnreachableException">Thrown if the broker cannot be reached after retries.</exception>
        public IConnection GetConnection()
        {
            if (!IsConnected)
            {
                lock (_connectionLock)
                {
                    // Double-check locking
                    if (!IsConnected)
                    {
                        _logger.LogInformation("No active RabbitMQ connection. Attempting to connect.");
                        TryConnect();
                    }
                }
            }

            if (_connection == null || !_connection.IsOpen)
            {
                 _logger.LogError("Failed to establish RabbitMQ connection after retries.");
                throw new BrokerUnreachableException("Failed to connect to RabbitMQ broker after configured retries.");
            }
            return _connection;
        }

        /// <summary>
        /// Creates a new RabbitMQ channel using the established connection.
        /// </summary>
        /// <returns>A new RabbitMQ model (channel).</returns>
        /// <exception cref="InvalidOperationException">Thrown if the connection is not open.</exception>
        public IModel CreateModel()
        {
            var connection = GetConnection(); // Ensures connection is attempted/established
            if (!connection.IsOpen)
            {
                _logger.LogError("Cannot create RabbitMQ channel: Connection is not open.");
                throw new InvalidOperationException("RabbitMQ connection is not open. Cannot create model.");
            }
            _logger.LogDebug("Creating new RabbitMQ channel.");
            return connection.CreateModel();
        }

        private void TryConnect(int retryAttempts = 5, int delayMilliseconds = 5000)
        {
            if (_disposed)
            {
                _logger.LogWarning("RabbitMqConnectionFactory is disposed. Cannot attempt connection.");
                return;
            }

            var factory = new ConnectionFactory()
            {
                HostName = _config.Hostname,
                Port = _config.Port,
                UserName = _config.Username,
                Password = _config.Password,
                VirtualHost = _config.VirtualHost,
                AutomaticRecoveryEnabled = true, // Enable automatic recovery
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10) // Interval for recovery attempts
            };

            if (_config.UseTls)
            {
                factory.Ssl = new SslOption
                {
                    Enabled = true,
                    // Further SSL options can be configured here, e.g., ServerName, CertPath, CertPassphrase
                    // ServerName = _config.Hostname, // Typically same as HostName
                    // AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors (Use with caution)
                };
                _logger.LogInformation("RabbitMQ connection configured to use TLS.");
            }

            for (int attempt = 1; attempt <= retryAttempts; attempt++)
            {
                try
                {
                    _logger.LogInformation("Attempting to connect to RabbitMQ ({Attempt}/{MaxAttempts})... Host: {Hostname}", attempt, retryAttempts, _config.Hostname);
                    _connection = factory.CreateConnection();

                    if (_connection.IsOpen)
                    {
                        _connection.ConnectionShutdown += OnConnectionShutdown;
                        _connection.CallbackException += OnCallbackException;
                        _connection.ConnectionBlocked += OnConnectionBlocked;
                        _connection.ConnectionUnblocked += OnConnectionUnblocked;
                        _logger.LogInformation("Successfully connected to RabbitMQ. Host: {Hostname}, ClientProvidedName: {ClientName}", _config.Hostname, _connection.ClientProvidedName);
                        return; // Success
                    }
                }
                catch (BrokerUnreachableException ex)
                {
                    _logger.LogError(ex, "Failed to connect to RabbitMQ (Attempt {Attempt}/{MaxAttempts}). Broker unreachable. Retrying in {Delay}ms...", attempt, retryAttempts, delayMilliseconds);
                }
                catch (AuthenticationFailureException ex)
                {
                    _logger.LogCritical(ex, "RabbitMQ authentication failed. Host: {Hostname}, User: {Username}. Aborting retries.", _config.Hostname, _config.Username);
                    throw; // Fatal error, do not retry
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred while connecting to RabbitMQ (Attempt {Attempt}/{MaxAttempts}). Retrying in {Delay}ms...", attempt, retryAttempts, delayMilliseconds);
                }

                if (attempt < retryAttempts)
                {
                    Thread.Sleep(delayMilliseconds);
                }
            }
            // If loop finishes, connection failed
             _logger.LogError("Failed to connect to RabbitMQ after {MaxAttempts} attempts.", retryAttempts);
        }

        private void OnConnectionShutdown(object? sender, ShutdownEventArgs reason)
        {
            _logger.LogWarning("RabbitMQ connection shut down. Reason: {Reason}. Attempting to reconnect if not disposed.", reason.ReplyText);
            // AutomaticRecoveryEnabled should handle reconnection. If it fails persistently, new GetConnection calls will trigger TryConnect.
            // Only attempt explicit reconnect if not disposed, to avoid issues during application shutdown.
            if (!_disposed)
            {
                // Optionally trigger a reconnect attempt here if specific conditions are met
                // Be careful not to create tight loops.
            }
        }

        private void OnCallbackException(object? sender, RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
        {
            _logger.LogError(e.Exception, "RabbitMQ callback exception. Detail: {Detail}", e.Detail);
        }

        private void OnConnectionBlocked(object? sender, RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
        {
            _logger.LogWarning("RabbitMQ connection blocked. Reason: {Reason}", e.Reason);
        }

        private void OnConnectionUnblocked(object? sender, EventArgs e)
        {
            _logger.LogInformation("RabbitMQ connection unblocked.");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _logger.LogInformation("Disposing RabbitMqConnectionFactory.");
                lock (_connectionLock)
                {
                    try
                    {
                        if (_connection != null)
                        {
                            // Unsubscribe events
                            _connection.ConnectionShutdown -= OnConnectionShutdown;
                            _connection.CallbackException -= OnCallbackException;
                            _connection.ConnectionBlocked -= OnConnectionBlocked;
                            _connection.ConnectionUnblocked -= OnConnectionUnblocked;

                            if (_connection.IsOpen)
                            {
                                _logger.LogDebug("Closing RabbitMQ connection.");
                                _connection.Close();
                            }
                            _connection.Dispose();
                            _connection = null;
                        }
                    }
                    catch (IOException ex) // Potentially thrown by Close() if connection is already lost badly
                    {
                        _logger.LogError(ex, "IOException during RabbitMQ connection close/dispose.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception during RabbitMQ connection close/dispose.");
                    }
                }
            }
            _disposed = true;
        }
    }
}