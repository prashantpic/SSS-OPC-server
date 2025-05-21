using ManagementService.Domain.SeedWork;
using ManagementService.Domain.Aggregates.BulkOperationJobAggregate.ValueObjects; // Reusing JobStatus
using System;
using System.Collections.Generic;
using System.Linq;
using ManagementService.Domain.Events;

namespace ManagementService.Domain.Aggregates.ConfigurationMigrationJobAggregate;

public class ConfigurationMigrationJob : Entity, IAggregateRoot
{
    public string FileName { get; private set; }
    public string SourceFormat { get; private set; } // e.g., "CSV", "XML"
    public JobStatus Status { get; private set; } // Reusing JobStatus from BulkOperationJob
    public string? ResultDetails { get; private set; } // Summary of outcome

    private readonly List<string> _validationMessages = new();
    public IReadOnlyCollection<string> ValidationMessages => _validationMessages.AsReadOnly();

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    // Private constructor for EF Core and factory methods
    private ConfigurationMigrationJob() { }

    public static ConfigurationMigrationJob Create(string fileName, string sourceFormat)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("File name cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(sourceFormat))
            throw new DomainException("Source format cannot be null or empty.");
        if (!IsValidSourceFormat(sourceFormat))
             throw new DomainException($"Unsupported source format: {sourceFormat}. Supported formats are CSV and XML.");


        var job = new ConfigurationMigrationJob
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            SourceFormat = sourceFormat.ToUpperInvariant(), // Standardize format case
            Status = JobStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        job.AddDomainEvent(new ConfigurationMigrationInitiatedEvent(job.Id, job.FileName));
        return job;
    }

    private static bool IsValidSourceFormat(string format)
    {
        return format.Equals("CSV", StringComparison.OrdinalIgnoreCase) ||
               format.Equals("XML", StringComparison.OrdinalIgnoreCase);
    }

    public void StartProcessing(JobStatus initialProcessingStatus) // e.g., Parsing, Uploaded
    {
        if (Status != JobStatus.Pending)
            throw new DomainException("Job must be in Pending state to start processing.");
        Status = initialProcessingStatus;
        // AddDomainEvent(...)
    }
    
    public void StartParsing()
    {
        if (Status != JobStatus.Pending && Status.Value != "Uploaded") // Assuming an "Uploaded" status might exist
            throw new DomainException("Job must be in Pending or Uploaded state to start parsing.");
        Status = JobStatus.FromString("Parsing"); // Example custom status string
    }

    public void StartTransforming()
    {
        if (Status.Value != "Parsing")
            throw new DomainException("Job must be in Parsing state to start transforming.");
        Status = JobStatus.FromString("Transforming");
    }
    
    public void StartValidating()
    {
        if (Status.Value != "Transforming")
            throw new DomainException("Job must be in Transforming state to start validating.");
        Status = JobStatus.FromString("Validating");
        _validationMessages.Clear(); // Clear previous messages if re-validating
    }

    public void StartSaving()
    {
        if (Status.Value != "Validating")
            throw new DomainException("Job must be in Validating state to start saving.");
        if (_validationMessages.Any())
            throw new DomainException("Cannot start saving as there are validation errors.");
        Status = JobStatus.FromString("Saving");
    }


    public void AddValidationMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        _validationMessages.Add(message);
    }

    public void AddValidationMessages(IEnumerable<string> messages)
    {
        foreach (var message in messages)
        {
            AddValidationMessage(message);
        }
    }

    public void Complete(string details)
    {
        if (Status == JobStatus.Completed || Status == JobStatus.Failed)
            throw new DomainException("Job is already completed or failed.");

        Status = JobStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        ResultDetails = details;
        // AddDomainEvent(new ConfigurationMigrationCompletedEvent(Id, true, details));
    }

    public void Fail(string reason)
    {
        if (Status == JobStatus.Completed || Status == JobStatus.Failed)
            throw new DomainException("Job is already completed or failed.");

        Status = JobStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
        ResultDetails = reason;
        // AddDomainEvent(new ConfigurationMigrationCompletedEvent(Id, false, reason));
    }
}