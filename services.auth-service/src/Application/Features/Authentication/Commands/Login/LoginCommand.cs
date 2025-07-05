using MediatR;

namespace AuthService.Application.Features.Authentication.Commands.Login;

/// <summary>
/// Represents a request to log in, containing the user's credentials.
/// This is a command in the CQRS pattern.
/// </summary>
/// <param name="Username">The user's username.</param>
/// <param name="Password">The user's password.</param>
public record LoginCommand(string Username, string Password) : IRequest<TokenResponse>;

/// <summary>
/// Represents the response containing authentication tokens.
/// </summary>
/// <param name="AccessToken">The JWT access token.</param>
/// <param name="ExpiresAt">The expiration date and time of the access token.</param>
public record TokenResponse(string AccessToken, DateTime ExpiresAt);