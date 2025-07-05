using DataService.Domain.Entities;
using DataService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace DataService.Application.Features.DataRetention.Commands;

// Placeholder command, will be moved to its own file.
public record UpdatePolicyCommand(Guid? PolicyId, string DataType, int RetentionDays, string Action, bool IsActive, string? ArchiveLocation) : IRequest;

// Placeholder for a proper Unit of Work implementation.
public interface IUnitOfWork : IDisposable
{
    IDataRetentionPolicyRepository Policies { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}


/// <summary>
/// Handles the creation or update of a data retention policy.
/// </summary>
public class UpdatePolicyCommandHandler : IRequestHandler<UpdatePolicyCommand>
{
    private readonly IDataRetentionPolicyRepository _policyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePolicyCommandHandler> _logger;

    public UpdatePolicyCommandHandler(
        IDataRetentionPolicyRepository policyRepository, 
        IUnitOfWork unitOfWork,
        ILogger<UpdatePolicyCommandHandler> logger)
    {
        _policyRepository = policyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UpdatePolicyCommand request, CancellationToken cancellationToken)
    {
        // 1. Validation
        if (!Enum.TryParse<DataType>(request.DataType, true, out var dataType))
            throw new ValidationException($"Invalid DataType: {request.DataType}");

        if (!Enum.TryParse<RetentionAction>(request.Action, true, out var retentionAction))
            throw new ValidationException($"Invalid Action: {request.Action}");

        if (request.RetentionDays <= 0)
            throw new ValidationException("RetentionDays must be a positive integer.");
        
        DataRetentionPolicy? policy;

        // 2. Create or Retrieve Entity
        if (request.PolicyId.HasValue)
        {
            _logger.LogInformation("Updating data retention policy with ID: {PolicyId}", request.PolicyId.Value);
            policy = await _policyRepository.GetByIdAsync(request.PolicyId.Value, cancellationToken);
            
            if (policy == null)
            {
                // In a real API, a specific NotFoundException would be thrown and handled by middleware.
                throw new KeyNotFoundException($"Policy with ID {request.PolicyId.Value} not found.");
            }
        }
        else
        {
            _logger.LogInformation("Creating a new data retention policy for DataType: {DataType}", request.DataType);
            policy = new DataRetentionPolicy { Id = Guid.NewGuid() };
            await _policyRepository.AddAsync(policy, cancellationToken);
        }

        // 3. Update Properties
        policy.DataType = dataType;
        policy.RetentionPeriodDays = request.RetentionDays;
        policy.Action = retentionAction;
        policy.IsActive = request.IsActive;
        policy.ArchiveLocation = request.ArchiveLocation;

        // The repository's Update method might not be needed if using EF Core's change tracker
        // But it's good practice for an explicit repository pattern.
        _policyRepository.Update(policy);
        
        // 4. Persist Changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully saved policy with ID: {PolicyId}", policy.Id);
    }
}