namespace ATS.Infrastructure.Events;

using ATS.Application.Common.Events;
using ATS.Domain.Shared;
using MediatR;

public sealed class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public MediatRDomainEventDispatcher(IMediator mediator) => _mediator = mediator;

    public async Task DispatchAndClearAsync(AggregateRoot aggregate, CancellationToken ct = default)
    {
        var events = aggregate.DomainEvents.ToList();
        aggregate.ClearDomainEvents();
        foreach (var domainEvent in events)
        {
            await _mediator.Publish(domainEvent, ct);
        }
    }
}
