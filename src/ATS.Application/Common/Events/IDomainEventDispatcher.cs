namespace ATS.Application.Common.Events;

using ATS.Domain.Shared;

public interface IDomainEventDispatcher
{
    Task DispatchAndClearAsync(AggregateRoot aggregate, CancellationToken ct = default);
}
