using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManagementService.Domain.SeedWork;

public abstract class Entity
{
    public Guid Id { get; protected set; }

    private List<INotification>? _domainEvents;
    public IReadOnlyCollection<INotification>? DomainEvents => _domainEvents?.AsReadOnly();

    protected Entity()
    {
        // ID is typically set by derived classes or persistence layer.
        // For new entities, Id should be generated in the factory method.
    }

    public void AddDomainEvent(INotification eventItem)
    {
        _domainEvents ??= new List<INotification>();
        _domainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(INotification eventItem)
    {
        _domainEvents?.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        // Transient entities are not considered equal to persisted ones
        // or to other transient ones unless their IDs happen to match.
        // A common approach: if Id is default (Guid.Empty), they are not equal unless ReferenceEquals.
        if (Id == Guid.Empty || other.Id == Guid.Empty)
            return false;

        return Id == other.Id;
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;

        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        // Consistent hash code based on Id.
        // If Id is Guid.Empty, could return base.GetHashCode() or a fixed value.
        return (GetType().ToString() + Id).GetHashCode();
    }
}