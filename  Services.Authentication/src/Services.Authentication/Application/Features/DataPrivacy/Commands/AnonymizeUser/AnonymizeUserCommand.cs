using MediatR;
using Microsoft.AspNetCore.Identity;
using Opc.System.Services.Authentication.Application.Interfaces;
using Opc.System.Services.Authentication.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Opc.System.Services.Authentication.Application.Features.DataPrivacy.Commands.AnonymizeUser;

/// <summary>
/// Represents the intent to anonymize a user's data to comply with data privacy regulations like GDPR's 'right to be forgotten'.
/// </summary>
/// <param name="UserId">The unique identifier of the user to anonymize.</param>
public record AnonymizeUserCommand([Required] Guid UserId) : IRequest<bool>;

/// <summary>
/// Handler for the AnonymizeUserCommand.
/// </summary>
public class AnonymizeUserCommandHandler : IRequestHandler<AnonymizeUserCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;

    public AnonymizeUserCommandHandler(UserManager<ApplicationUser> userManager, IAuditService auditService)
    {
        _userManager = userManager;
        _auditService = auditService;
    }

    public async Task<bool> Handle(AnonymizeUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return false;
        }

        // Anonymize Personally Identifiable Information (PII)
        user.FirstName = "Anonymized";
        user.LastName = "User";
        user.Email = $"anonymized_{user.Id}@opc.system";
        user.NormalizedEmail = user.Email.ToUpperInvariant();
        user.UserName = $"anonymized_{user.Id}";
        user.NormalizedUserName = user.UserName.ToUpperInvariant();
        user.PhoneNumber = null;

        // Disable the account and invalidate credentials/sessions
        user.IsActive = false;
        user.PasswordHash = null; // Removes the ability to log in with a password
        user.SecurityStamp = Guid.NewGuid().ToString(); // Invalidates existing cookies/tokens

        user.UpdatedAt = DateTimeOffset.UtcNow;
        
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            await _auditService.LogEventAsync(
                "UserAnonymized", 
                "Success", 
                new { OriginalUserId = user.Id, AnonymizedUsername = user.UserName },
                subjectId: user.Id.ToString()
            );
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            await _auditService.LogEventAsync(
                "UserAnonymizationFailure",
                "Failure",
                new { OriginalUserId = user.Id, Errors = errors },
                subjectId: user.Id.ToString()
            );
        }

        return result.Succeeded;
    }
}