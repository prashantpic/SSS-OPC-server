using OPC.Client.Core.Domain.ValueObjects;
using OPC.Client.Core.Domain.Enums;
using System;
using Microsoft.Extensions.Logging;
using Opc.Ua; // For Opc.Ua.Variant and Opc.Ua.TypeInfo
// For DA/HDA, if using OPC Foundation libraries, specific types might be needed
// e.g., Opc.Da.ItemValue, Opc.Hda.ItemValue
// For simplicity, this example focuses on UA and basic .NET type conversions.

namespace OPC.Client.Core.Domain.DomainServices
{
    /// <summary>
    /// Domain service for converting diverse OPC data types (variants) to .NET types and vice-versa.
    /// Handles complex structures and arrays. Provides stateless conversion logic.
    /// REQ-CSVC-001
    /// </summary>
    public class OpcVariantConverter
    {
        private readonly ILogger<OpcVariantConverter> _logger;

        public OpcVariantConverter(ILogger<OpcVariantConverter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Converts an OPC UA Variant to a .NET object.
        /// </summary>
        /// <param name="variant">The OPC UA Variant.</param>
        /// <returns>The converted .NET object, or null if conversion fails or variant is null/empty.</returns>
        public object? ConvertFromOpcUaVariant(Variant variant)
        {
            if (variant == Variant.Null)
            {
                return null;
            }

            try
            {
                // The .Value property of Opc.Ua.Variant already attempts to convert to the most appropriate .NET type.
                // For complex types (structures, arrays), it might return specific Opc.Ua types (e.g., ExtensionObject, Matrix).
                // Further processing might be needed for those.
                return variant.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting OPC UA Variant (Type: {VariantType}) to .NET type.", variant.TypeInfo?.BuiltInType);
                return null; // Or re-throw as a custom conversion exception
            }
        }

        /// <summary>
        /// Converts a .NET object to an OPC UA Variant.
        /// </summary>
        /// <param name="value">The .NET object.</param>
        /// <returns>An OPC UA Variant, or Variant.Null if the input value is null.</returns>
        public Variant ConvertToOpcUaVariant(object? value)
        {
            if (value == null)
            {
                return Variant.Null;
            }

            try
            {
                // Opc.Ua.Variant constructor handles most basic .NET types.
                // For custom structures, they need to be IEncodeable or have a TypeInfo.
                return new Variant(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting .NET type {NetType} to OPC UA Variant.", value.GetType().FullName);
                return Variant.Null; // Or re-throw
            }
        }

        /// <summary>
        /// Converts a generic OPC protocol value (could be from DA, XML-DA, HDA) to a .NET object.
        /// This is a placeholder and needs specific implementation per protocol if libraries return custom types.
        /// </summary>
        /// <param name="opcValue">The value from the OPC protocol.</param>
        /// <param name="protocol">The OPC protocol type.</param>
        /// <returns>The converted .NET object.</returns>
        public object? ConvertFromGenericOpcValue(object? opcValue, OpcProtocolType protocol)
        {
            if (opcValue == null) return null;

            _logger.LogTrace("Converting value from OPC Protocol {Protocol}, Original Type: {Type}", protocol, opcValue.GetType().FullName);

            // For DA/HDA, if using OPC Foundation COM wrappers, values are often already .NET types.
            // If specific library types like Opc.Da.ItemValue are used, extract .Value.
            // This method assumes the raw value object is passed.

            // Example for Opc.Da.ItemValue (if it were passed directly)
            // if (opcValue is Opc.Da.ItemValue daItemValue) return daItemValue.Value;

            // For XML-DA, values from SOAP responses are also typically .NET types.

            // If no specific conversion is needed for the given protocol and type, return as is.
            // This is highly dependent on the actual libraries used for DA/XML-DA/HDA.
            // The Opc.Ua.Variant conversion is more standardized via the SDK.
            return opcValue;
        }

        /// <summary>
        /// Converts a .NET object to a generic OPC protocol value.
        /// Placeholder, specific protocols might need specific wrapping.
        /// </summary>
        /// <param name="netValue">The .NET object.</param>
        /// <param name="protocol">The target OPC protocol type.</param>
        /// <returns>The value formatted for the target OPC protocol.</returns>
        public object? ConvertToGenericOpcValue(object? netValue, OpcProtocolType protocol)
        {
            if (netValue == null) return null;

            // For DA/HDA/XML-DA, direct use of .NET types is often sufficient.
            // If a specific protocol requires wrapping (e.g., into an Opc.Da.ItemValue object),
            // that logic would be in the respective communicator.
            // This service focuses on the *value* itself, not the container.
            return netValue;
        }
    }
}