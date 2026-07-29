namespace RestaurantSystem.Application.Abstractions.Messaging;

public sealed record IntegrationEventEnvelope(
    Guid MessageId,
    string Type,
    string PayloadJson,
    DateTime OccurredOnUtc,
    IReadOnlyDictionary<string, string>? Headers = null
);
