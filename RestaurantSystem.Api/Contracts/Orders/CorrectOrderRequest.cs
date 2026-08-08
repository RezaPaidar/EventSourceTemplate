namespace RestaurantSystem.Api.Contracts.Orders;

public sealed record CorrectOrderRequest(
    int NewQuantity,
    Guid OriginalEventId,
    string Reason);
