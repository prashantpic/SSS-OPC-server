namespace SharedKernel.Exceptions;

/// <summary>
/// Base exception for errors that occur within the domain layer,
/// representing a violation of a business rule or invariant.
/// </summary>
public class DomainException : Exception
{
    public DomainException() { }

    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}