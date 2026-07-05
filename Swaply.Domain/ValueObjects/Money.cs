namespace Swaply.Domain.ValueObjects;

public record Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency = "VND") => new(0, currency);
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new ArgumentException("Currencies must match to perform addition.");
        }
        return new Money(Amount + other.Amount, Currency);
    }
}
