namespace RestaurantSystem.Infrastructure.Persistence.EventStore.Models
{
    public class InboxMessage
    {
        public Guid Id { get; set; }
        public required string Type { get; set; }
        public required string Payload { get; set; }
        public DateTime OccurredOnUtc { get; set; }
        public DateTime? ProcessedAtUtc { get; set; }
        public string? Error { get; set; }
    }

}