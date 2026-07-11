namespace Swaply.Domain.Exceptions;

public class InvalidExchangeStateException : DomainException
{
    public InvalidExchangeStateException(string message) : base(message)
    {
    }
}
