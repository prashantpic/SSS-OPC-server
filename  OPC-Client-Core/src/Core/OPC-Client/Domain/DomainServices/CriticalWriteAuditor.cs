using OPC.Client.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using OPC.Client.Core.Application; // For CriticalWriteAuditConfiguration and TagConfiguration

namespace OPC.Client.Core.Domain.DomainServices
{
    /// <summary>
    /// Domain service responsible for logging critical write operations.
    /// Handles the identification and logging of write operations to critical tags,
    /// including user credentials, timestamp, tag ID, old/new values.
    /// REQ-CSVC-009
    /// </summary>
    public class CriticalWriteAuditor
    {
        private readonly ILogger<CriticalWriteAuditor> _auditLogger; // Use a dedicated logger category for audit
        private readonly CriticalWriteAuditConfiguration? _config;
        private readonly HashSet<string> _criticalTagNodeIds = new HashSet<string>();

        public CriticalWriteAuditor(ILogger<CriticalWriteAuditor> auditLogger,
                                    CriticalWriteAuditConfiguration? config,
                                    IEnumerable<TagConfiguration>? allTagConfigurations) // Pass all tags to identify critical ones
        {
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _config = config;

            if (_config?.EnableAuditing == true)
            {
                if (_config.CriticalTagIds != null && _config.CriticalTagIds.Any())
                {
                    // If specific TagIds (internal identifiers) are provided in config
                    if (allTagConfigurations != null)
                    {
                        foreach (var tagId in _config.CriticalTagIds)
                        {
                            var tag = allTagConfigurations.FirstOrDefault(t => t.TagId == tagId);
                            if (tag != null)
                            {
                                _criticalTagNodeIds.Add(tag.NodeAddress.ToString());
                            }
                            else
                            {
                                _auditLogger.LogWarning("CriticalTagId '{TagId}' in audit configuration not found in global tag configurations.", tagId);
                            }
                        }
                    }
                    else
                    {
                         _auditLogger.LogWarning("CriticalTagIds are configured for auditing, but no global tag configurations provided to resolve NodeAddresses.");
                    }
                }
                else if (allTagConfigurations != null) // Fallback to IsCriticalWrite flag in TagConfiguration
                {
                    foreach (var tag in allTagConfigurations.Where(t => t.IsCriticalWrite))
                    {
                        _criticalTagNodeIds.Add(tag.NodeAddress.ToString());
                    }
                }
                _auditLogger.LogInformation("Critical Write Auditor initialized. Auditing enabled for {Count} critical tags.", _criticalTagNodeIds.Count);
            }
            else
            {
                _auditLogger.LogInformation("Critical Write Auditor initialized. Auditing is disabled.");
            }
        }

        /// <summary>
        /// Audits a write operation if the target tag is configured as critical.
        /// </summary>
        /// <param name="connectionId">Identifier of the OPC connection.</param>
        /// <param name="tagValue">The OpcDataValue that was written (contains NodeAddress and new value).</param>
        /// <param name="oldValue">The value of the tag before the write (if available).</param>
        /// <param name="userIdentity">Identifier of the user/system performing the write (if available).</param>
        public void AuditWrite(string connectionId, OpcDataValue tagValue, object? oldValue, string? userIdentity)
        {
            if (_config == null || !_config.EnableAuditing)
            {
                return; // Auditing disabled
            }

            string nodeKey = tagValue.NodeAddress.ToString();
            if (_criticalTagNodeIds.Contains(nodeKey))
            {
                _auditLogger.LogWarning(
                    "CRITICAL WRITE: User='{UserIdentity}', Connection='{ConnectionId}', Tag='{TagNodeAddress}', OldValue='{OldValue}', NewValue='{NewValue}', Timestamp='{TimestampUtc}', Quality='{Quality}'",
                    userIdentity ?? "N/A",
                    connectionId,
                    tagValue.NodeAddress,
                    oldValue ?? "N/A",
                    tagValue.Value ?? "N/A",
                    tagValue.Timestamp.ToUniversalTime(),
                    tagValue.Quality
                );
                // This log should go to a separate, secure audit trail as per REQ-CSVC-009.
                // Serilog can be configured with multiple sinks for this purpose.
            }
        }
    }
}