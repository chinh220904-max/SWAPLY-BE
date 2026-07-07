namespace Swaply.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "VND";

    public Money() { }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(string currency = "VND") => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new ArgumentException("Currencies must match to perform addition.", nameof(other));
        }
        return new Money(Amount + other.Amount, Currency);
    }
}
