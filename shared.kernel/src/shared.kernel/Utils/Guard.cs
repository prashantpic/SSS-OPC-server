using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SharedKernel.Utils;

/// <summary>
/// Provides helper methods for argument and state validation.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Throws an ArgumentNullException if the input argument is null.
    /// </summary>
    /// <param name="argument">The argument to check.</param>
    /// <param name="paramName">The name of the parameter, captured automatically.</param>
    /// <exception cref="ArgumentNullException">Thrown if argument is null.</exception>
    public static void AgainstNull([NotNull] object? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    /// <summary>
    /// Throws an ArgumentException if the input string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="argument">The string to check.</param>
    /// <param name="paramName">The name of the parameter, captured automatically.</param>
    /// <exception cref="ArgumentException">Thrown if the string is null, empty, or whitespace.</exception>
    public static void AgainstNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new ArgumentException("Argument cannot be null, empty, or whitespace.", paramName);
        }
    }
}