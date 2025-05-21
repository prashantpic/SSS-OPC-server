namespace OPC.Client.Core.Domain.ValueObjects
{
    using System;

    /// <summary>
    /// Value object representing a unique address or identifier for an OPC node/tag.
    /// Provides an immutable and strongly-typed representation of an OPC node's address or identifier,
    /// ensuring consistency and clarity.
    /// Implements REQ-CSVC-001, REQ-CSVC-002, REQ-CSVC-003.
    /// </summary>
    public record NodeAddress
    {
        /// <summary>
        /// The identifier of the node (e.g., tag name for DA/XML-DA, NodeId string for UA).
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// The namespace index (primarily relevant for OPC UA, can be null for other protocols).
        /// </summary>
        public ushort? NamespaceIndex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeAddress"/> record.
        /// </summary>
        /// <param name="identifier">The identifier string. Cannot be null or empty.</param>
        /// <param name="namespaceIndex">The namespace index (optional for UA, usually null for others).</param>
        /// <exception cref="ArgumentNullException">Thrown if identifier is null.</exception>
        /// <exception cref="ArgumentException">Thrown if identifier is empty or whitespace.</exception>
        public NodeAddress(string identifier, ushort? namespaceIndex = null)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Identifier cannot be empty or whitespace.", nameof(identifier));

            Identifier = identifier;
            NamespaceIndex = namespaceIndex;
        }

        /// <summary>
        /// Returns a string representation of the NodeAddress.
        /// For OPC UA, this typically includes the namespace index if present (e.g., "ns=2;s=MyTag").
        /// For other protocols, it's usually just the identifier.
        /// </summary>
        /// <returns>A string representation of the node address.</returns>
        public override string ToString()
        {
            // This is a common UA format. Other protocols might just use Identifier.
            // For generic representation, decide on a consistent format.
            if (NamespaceIndex.HasValue)
            {
                // A more robust UA NodeId parser/formatter might be needed for complex NodeIds
                // For simplicity, we'll use a basic format.
                return $"ns={NamespaceIndex.Value};s={Identifier}"; // Example, actual UA NodeId parsing is more complex for all types
            }
            return Identifier;
        }
    }
}