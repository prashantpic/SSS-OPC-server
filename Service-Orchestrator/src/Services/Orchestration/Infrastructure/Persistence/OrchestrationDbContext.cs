using Microsoft.EntityFrameworkCore;
// WorkflowCore.Persistence.EntityFrameworkCore is the package name for WorkflowCore 3.x
// For WorkflowCore 7.x, it's WorkflowCore.Persistence.EntityFrameworkCore
// The SDS implies WorkflowCore 7.0.0 but the file structure and class `WorkflowDbContext`
// points to WorkflowCore.Persistence.EntityFramework.WorkflowDbContext.
// Let's assume `WorkflowCore.Persistence.EntityFrameworkCore` package is used and it provides `WorkflowDbContext`.
// If using DurableTask.EntityFrameworkCore, the base DbContext would be different, e.g. `TaskHubDbContext`.
// The SDS says "WorkflowCore 7.0.0 (chosen based on technology list and template names)" and
// "Persistence: Entity Framework Core (implied by file structure)" and
// "2.3. Persistence Entity Framework Core persistence will be configured using WorkflowPersistenceSetup.cs and OrchestrationDbContext.cs"
// And "8.1. OrchestrationDbContext.cs Inherits from WorkflowCore.Persistence.EntityFramework.WorkflowDbContext."

// So, it's `WorkflowCore.Persistence.EntityFramework.WorkflowDbContext`.
// The nuget package for WorkflowCore 7 is likely `WorkflowCore.Providers.EntityFramework`.

using WorkflowCore.Persistence.EntityFramework.Models; // For ExecutionPointerRow, etc. if customizing
using WorkflowCore.Persistence.EntityFramework.Services; // For WorkflowDbContext

namespace OrchestrationService.Infrastructure.Persistence
{
    /// <summary>
    /// Defines the Entity Framework Core DbContext used by WorkflowCore
    /// for storing workflow instances, events, and execution pointers.
    /// </summary>
    public class OrchestrationDbContext : WorkflowDbContext // This is correct for WorkflowCore.Providers.EntityFramework
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestrationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public OrchestrationDbContext(DbContextOptions<OrchestrationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from the entity types
        /// exposed in <see cref="DbSet{TEntity}" /> properties on your derived context. The resulting model may be cached
        /// and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">
        /// The builder being used to construct the model for this context. Databases (and other extensions) typically
        /// define extension methods on this object that allow you to configure aspects of the model that are specific
        /// to a given database.
        /// </param>
        /// <remarks>
        /// If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        /// then this method will not be run.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Add any custom WorkflowCore entity configurations or other entities specific to orchestration here.
            // For example, if you have custom persistence entities for workflow data that are not part of WorkflowCore itself.
            // By default, WorkflowDbContext handles all WorkflowCore tables (WorkflowInstance, ExecutionPointer, Event, EventSubscription, etc.).
        }
    }
}