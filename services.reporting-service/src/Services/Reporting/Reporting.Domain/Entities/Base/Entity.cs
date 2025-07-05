namespace Reporting.Domain.Entities.Base;

/// <summary>
/// Base class for all domain entities.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class Entity<TId> where TId : notnull
{
    /// <summary>
    /// Gets the identifier of the entity.
    /// </summary>
    public TId Id { get; protected set; }

    protected Entity(TId id)
    {
        Id = id;
    }

    // Protected parameterless constructor for EF Core
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected Entity() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}