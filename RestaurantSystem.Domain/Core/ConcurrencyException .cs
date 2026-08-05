namespace RestaurantSystem.Domain.Core;

public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
}
