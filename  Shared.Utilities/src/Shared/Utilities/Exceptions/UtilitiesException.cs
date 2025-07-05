using System;

namespace Opc.System.Shared.Utilities.Exceptions
{
    /// <summary>
    /// The base exception for errors that occur within the Shared.Utilities library.
    /// It allows for differentiated error handling by consumers of the library.
    /// </summary>
    public class UtilitiesException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UtilitiesException"/> class.
        /// </summary>
        public UtilitiesException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UtilitiesException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public UtilitiesException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UtilitiesException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public UtilitiesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}