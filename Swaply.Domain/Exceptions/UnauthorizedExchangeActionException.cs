namespace Swaply.Domain.Exceptions;

public class UnauthorizedExchangeActionException : DomainException
{
    public UnauthorizedExchangeActionException(string message) : base(message)
    {
    }
}
