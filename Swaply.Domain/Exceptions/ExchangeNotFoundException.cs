namespace Swaply.Domain.Exceptions;

public class ExchangeNotFoundException : DomainException
{
    public Guid ExchangeId { get; }

    public ExchangeNotFoundException(Guid exchangeId)
        : base($"Exchange with ID {exchangeId} was not found.")
    {
        ExchangeId = exchangeId;
    }
}
