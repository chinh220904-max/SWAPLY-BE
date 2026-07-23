using Swaply.Domain.Entities;

namespace Swaply.Application.ExchangeManagement;

public interface IExchangeService
{
    Task<ExchangeDto> CreateExchangeAsync(CreateExchangeRequest request, Guid proposerId, CancellationToken cancellationToken = default);
    Task<ExchangeDto?> GetExchangeByIdAsync(Guid id, Guid requesterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExchangeDto>> GetMyExchangesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExchangeDto>> GetOutgoingExchangesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExchangeDto>> GetIncomingExchangesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ExchangeDto> AcceptExchangeAsync(Guid exchangeId, Guid requesterId, CancellationToken cancellationToken = default);
    Task<ExchangeDto> RejectExchangeAsync(Guid exchangeId, Guid requesterId, CancellationToken cancellationToken = default);
    Task<ExchangeDto> CancelExchangeAsync(Guid exchangeId, Guid requesterId, CancellationToken cancellationToken = default);
    Task<ExchangeDto> CompleteExchangeAsync(Guid exchangeId, Guid requesterId, CancellationToken cancellationToken = default);
}
