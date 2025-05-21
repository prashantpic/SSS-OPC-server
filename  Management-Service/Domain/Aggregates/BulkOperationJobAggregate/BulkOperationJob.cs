using ManagementService.Domain.SeedWork;
using ManagementService.Domain.Aggregates.BulkOperationJobAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using ManagementService.Domain.Events;
using System.Text.Json; // For ParametersJson

namespace ManagementService.Domain.Aggregates.BulkOperationJobAggregate;

public class BulkOperationJob : Entity, IAggregateRoot
{
    public BulkOperationType OperationType { get; private set; }
    public JobStatus Status { get; private set; }
    public string ParametersJson { get; private set; } // Parameters for the specific operation type
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? ResultDetails { get; private set; } // Summary of results

    private readonly List<BulkOperationTaskDetail> _progressDetails = new();
    public IReadOnlyCollection<BulkOperationTaskDetail> ProgressDetails => _progressDetails.AsReadOnly();

    // Private constructor for EF Core and factory methods
    private BulkOperationJob() { }

    public static BulkOperationJob Create(BulkOperationType operationType, object parameters)
    {
        ArgumentNullException.ThrowIfNull(operationType);
        ArgumentNullException.ThrowIfNull(parameters);

        var job = new BulkOperationJob
        {
            Id = Guid.NewGuid(),
            OperationType = operationType,
            ParametersJson = JsonSerializer.Serialize(parameters),
            Status = JobStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
        // job.AddDomainEvent(new BulkOperationCreatedEvent(job.Id, job.OperationType.Value));
        return job;
    }

    public void Start()
    {
        if (Status != JobStatus.Pending)
            throw new DomainException("Job must be in Pending state to start.");
        Status = JobStatus.InProgress;
        AddDomainEvent(new BulkOperationStartedEvent(Id, OperationType.Value));
    }

    public void Complete(string details)
    {
        if (Status == JobStatus.Completed || Status == JobStatus.Failed)
            throw new DomainException("Job is already completed or failed.");

        Status = JobStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        ResultDetails = details;
        // AddDomainEvent(new BulkOperationCompletedEvent(Id, true, details));
    }

    public void Fail(string reason)
    {
        if (Status == JobStatus.Completed || Status == JobStatus.Failed)
            throw new DomainException("Job is already completed or failed.");

        Status = JobStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
        ResultDetails = reason;
        // AddDomainEvent(new BulkOperationCompletedEvent(Id, false, reason));
    }

    public void AddTask(Guid clientInstanceId, string initialStatus, string? details = null)
    {
        if (_progressDetails.Any(pd => pd.ClientInstanceId == clientInstanceId))
            throw new DomainException($"Task for client {clientInstanceId} already exists in this job.");

        var taskDetail = BulkOperationTaskDetail.Create(clientInstanceId, initialStatus, details);
        _progressDetails.Add(taskDetail);
    }

    public void UpdateTaskStatus(Guid clientInstanceId, string newStatus, string? details = null)
    {
        var taskDetail = _progressDetails.FirstOrDefault(pd => pd.ClientInstanceId == clientInstanceId);
        if (taskDetail == null)
            throw new DomainException($"Task for client {clientInstanceId} not found in this job.");

        taskDetail.UpdateStatus(newStatus, details);

        // Optional: Check if all tasks are done to automatically complete/fail the job
        if (Status == JobStatus.InProgress && _progressDetails.All(pd => pd.IsTerminal))
        {
            if (_progressDetails.Any(pd => pd.Status == "Failed")) // Assuming "Failed" is a terminal status string
            {
                Fail($"Completed with {_progressDetails.Count(pd => pd.Status == "Failed")} failures.");
            }
            else
            {
                Complete($"Completed successfully for all {_progressDetails.Count} tasks.");
            }
        }
    }
    
    public void UpdateDetails(string details)
    {
        this.ResultDetails = details; // Can be used to append ongoing details or set final summary
    }
}