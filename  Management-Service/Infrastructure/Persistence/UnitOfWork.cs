using ManagementService.Application.Abstractions.Data;
using ManagementService.Domain.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementService.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ManagementDbContext _dbContext;
    private readonly IMediator _mediator;
    private IDbContextTransaction? _currentTransaction;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(ManagementDbContext dbContext, IMediator mediator, ILogger<UnitOfWork> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Attempting to save changes to the database.");
        // Dispatch Domain Events before saving changes if not handled by DbContext itself
        // Note: The current DbContext is set up to dispatch after its own SaveChangesAsync.
        // This SaveChangesAsync call here might trigger that, or we explicitly call DispatchDomainEventsAsync if DbContext doesn't.
        // Let's ensure events are dispatched. If DbContext already does it, _mediator could be removed here.
        // The previous generated DbContext SaveChangesAsync handles dispatching.
        // So, this method just calls the DbContext's SaveChangesAsync.
        var result = await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successfully saved {Count} changes to the database.", result);
        return result;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            _logger.LogWarning("BeginTransactionAsync called while a transaction is already in progress.");
            return; // Or throw InvalidOperationException("A transaction is already in progress.");
        }
        _logger.LogInformation("Beginning a new database transaction.");
        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            _logger.LogError("CommitTransactionAsync called but no transaction is active.");
            throw new InvalidOperationException("No transaction in progress to commit.");
        }

        try
        {
            _logger.LogInformation("Attempting to commit database transaction.");
            // Ensure all changes are saved before committing the transaction
            // The DbContext.SaveChangesAsync internally handles event dispatching now.
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Database transaction committed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing database transaction. Rolling back.");
            await RollbackTransactionInternalAsync(cancellationToken); // Internal rollback without disposing yet
            throw; // Re-throw the exception after rollback
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            _logger.LogWarning("RollbackTransactionAsync called but no transaction is active.");
            return;
        }
        _logger.LogInformation("Attempting to roll back database transaction.");
        await RollbackTransactionInternalAsync(cancellationToken);
        await DisposeTransactionAsync();
    }

    private async Task RollbackTransactionInternalAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_currentTransaction != null) // Check again, could be called by Commit's catch
            {
                 await _currentTransaction.RollbackAsync(cancellationToken);
                 _logger.LogInformation("Database transaction rolled back.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during transaction rollback.");
            // Potentially re-throw or handle critical rollback failure
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
            _logger.LogDebug("Database transaction disposed.");
        }
    }

    public void Dispose()
    {
        _logger.LogDebug("Disposing UnitOfWork.");
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _currentTransaction?.Dispose(); // Ensure transaction is disposed
            _dbContext.Dispose();
        }
    }
}