using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Opc.System.Services.Authentication.Domain.Entities;

namespace Opc.System.Services.Authentication.Application.Features.AccessReview.Queries.GenerateAccessReviewReport;

/// <summary>
/// Represents a request to generate a user access review report. The handler gathers and structures the necessary data.
/// </summary>
public record GenerateAccessReviewReportQuery : IRequest<AccessReviewReportDto>;

/// <summary>
/// DTO for the overall access review report.
/// </summary>
public record AccessReviewReportDto(List<UserAccessDetailsDto> Users);

/// <summary>
/// DTO representing the access details for a single user.
/// </summary>
public record UserAccessDetailsDto(
    Guid UserId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    DateTimeOffset CreatedAt,
    List<string> Roles);

/// <summary>
/// Handler for the GenerateAccessReviewReportQuery.
/// </summary>
public class GenerateAccessReviewReportQueryHandler : IRequestHandler<GenerateAccessReviewReportQuery, AccessReviewReportDto>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GenerateAccessReviewReportQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AccessReviewReportDto> Handle(GenerateAccessReviewReportQuery request, CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        var userAccessDetailsList = new List<UserAccessDetailsDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userDetail = new UserAccessDetailsDto(
                user.Id,
                user.UserName ?? "N/A",
                user.Email ?? "N/A",
                user.FirstName,
                user.LastName,
                user.IsActive,
                user.CreatedAt,
                roles.ToList()
            );
            userAccessDetailsList.Add(userDetail);
        }

        return new AccessReviewReportDto(userAccessDetailsList);
    }
}