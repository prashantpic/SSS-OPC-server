using Microsoft.Extensions.Logging;
using OrchestrationService.Infrastructure.HttpClients.ManagementService;
using OrchestrationService.Workflows.ReportGeneration.Models; // Assuming ReportGenerationSagaData, ReportValidatedEvent are here or a shared DTOs namespace
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Primitives; // For WaitFor activity

namespace OrchestrationService.Workflows.ReportGeneration.Activities
{
    /// <summary>
    /// Workflow activity to manage report validation and sign-off.
    /// Waits for an external event 'ReportValidatedEvent'.
    /// Corresponds to REQ-7-022.
    /// </summary>
    public class ReportValidationStepActivity : StepBodyAsync // Or could be a custom step using WaitFor
    {
        private readonly IManagementServiceClient _managementServiceClient; // To check validator roles
        private readonly ILogger<ReportValidationStepActivity> _logger;

        // Input from SagaData: ReportId, GeneratedDocumentUri
        // Output to SagaData: ValidationStatus

        // EventName and EventKey are used by WorkflowCore's WaitFor step if this class wraps it or if this is used in conjunction.
        // For a custom activity that waits, it might manage its own event subscription logic or polling.
        // Let's assume this activity IS the point where workflow pauses for an external event.

        public string ReportId { get; set; } // Will be mapped from SagaData by workflow definition
        public string ValidationStatus { get; set; } // Will be mapped back to SagaData

        public ReportValidationStepActivity(
            IManagementServiceClient managementServiceClient,
            ILogger<ReportValidationStepActivity> logger)
        _managementServiceClient = managementServiceClient;
            _logger = logger;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var sagaData = (ReportGenerationSagaData)context.Workflow.Data;
            this.ReportId = sagaData.ReportId; // Ensure ReportId is set for context

            _logger.LogInformation("Report validation step initiated for ReportId: {ReportId}. Waiting for 'ReportValidatedEvent'.", sagaData.ReportId);
            sagaData.CurrentStatus = "Validating";
            sagaData.ValidationStatus = "Pending"; // Initial status

            // Logic to check if validation is actually required based on sagaData.RequestParameters.RequiresValidation
            if (!sagaData.RequestParameters.RequiresValidation)
            {
                _logger.LogInformation("Validation not required for ReportId: {ReportId}. Skipping.", sagaData.ReportId);
                sagaData.ValidationStatus = "NotRequired";
                this.ValidationStatus = "NotRequired";
                return ExecutionResult.Next();
            }

            // Here, the workflow needs to pause. A common pattern is to use WorkflowCore's WaitFor primitive.
            // This activity itself doesn't pause. It would typically be used like:
            // .Then<ReportValidationStepActivity>() // To setup something (e.g. send notification for validation)
            // .WaitFor("ReportValidatedEvent", (data, context) => data.ReportId == context.Workflow.Id) // Workflow pauses here
            // .Output(data => data.ValidationStatus, step => step.EventData.ValidationOutcome) // Map event data back
            // .Then<ProcessValidationResultActivity>() // To process the outcome
            //
            // However, the SDS states "ReportValidationStepActivity: Wait for external event".
            // This implies this activity itself might encapsulate the waiting logic or be a trigger point for it.
            // For a self-contained activity that waits, it would need to return ExecutionResult.WaitForActivity(...)
            // or handle event subscription itself, which is more complex with IWorkflowHost.PublishEvent.

            // Let's assume this activity's purpose is to set the stage and the waiting is handled
            // by a .WaitFor in the workflow definition *after* this step if this step sends a notification.
            // OR, if this step itself is meant to be the "WaitFor" step, it should be implemented differently.
            // Given the "Output: ValidationStatus" for this activity, it implies it *receives* the status.
            // So, it's likely the step that *processes* the event after a WaitFor.

            // Let's reconsider: "Workflow pauses here until validation is complete or times out."
            // This means this activity *should* be the one that makes the workflow wait.
            // This can be done by this activity returning ExecutionResult.WaitForActivity(...)
            // or by designing it to be used with a Saga/sub-workflow pattern that manages the wait.

            // A simpler interpretation for a single activity: this activity does nothing but the workflow
            // definition uses .WaitFor("ReportValidatedEvent", keySelector, eventTimeout) right after (or this IS the WaitFor).
            // If this class *is* the `WaitFor` activity, its structure would be different, inheriting from `Activity`
            // and overriding `Execute` to return `ExecutionResult.WaitForActivity`.

            // Let's assume the activity that "waits" is implicitly the `WaitFor` primitive,
            // and `ReportValidationStepActivity` is more about *processing* the result of that wait,
            // or is just a placeholder for where `WaitFor` should be used.
            // Given the "Output: ValidationStatus", it's likely the step that gets populated by the event.

            // If this step IS the "WaitFor" step:
            if (context.PersistenceData == null) // First time execution
            {
                _logger.LogInformation("ReportId: {ReportId} is now awaiting external validation event 'ReportValidatedEvent'.", sagaData.ReportId);
                // Here you could potentially call ManagementService to log that validation is pending or get validator info
                // var validatorInfo = await _managementServiceClient.GetUserRolesAsync(...); // Example
                return ExecutionResult.WaitForActivity("ReportValidatedEvent", sagaData.ReportId, System.DateTime.UtcNow.AddDays(7)); // Event Name, Event Key, Effective Date for timeout
            }
            
            // This part executes when the event is received (if PersistenceData is not null)
            if (context.PersistenceData is ReportValidatedEventPayload validatedEventPayload)
            {
                _logger.LogInformation("Received 'ReportValidatedEvent' for ReportId: {ReportId} with Status: {ValidationOutcome}", sagaData.ReportId, validatedEventPayload.ValidationOutcome);
                sagaData.ValidationStatus = validatedEventPayload.ValidationOutcome;
                this.ValidationStatus = sagaData.ValidationStatus; // Set output property

                if (sagaData.ValidationStatus == "Rejected")
                {
                    sagaData.FailureReason = "Report validation rejected.";
                    sagaData.CurrentStatus = "ValidationFailed";
                    _logger.LogWarning("Report {ReportId} was rejected during validation.", sagaData.ReportId);
                    return ExecutionResult.Outcome("ValidationRejected"); // Custom outcome for branching
                }
                sagaData.CurrentStatus = "Validated";
                return ExecutionResult.Next();
            }

            _logger.LogWarning("ReportValidationStepActivity resumed for ReportId: {ReportId} but persistence data was not the expected ReportValidatedEventPayload or was null.", sagaData.ReportId);
            sagaData.FailureReason = "Validation step resumed with unexpected data.";
            sagaData.CurrentStatus = "ValidationFailed";
            return ExecutionResult.Outcome("Error"); // Or handle as a timeout if no event received
        }
    }

    // Placeholder for the event payload. This should be in a shared DTOs/Events namespace.
    public class ReportValidatedEventPayload
    {
        public string ReportId { get; set; }
        public string ValidationOutcome { get; set; } // e.g., "Approved", "Rejected"
        public string ValidatedBy { get; set; }
        public System.DateTime ValidationTimestamp { get; set; }
        public string Comments { get; set; }
    }
}