using Swaply.Domain.DomainServices;
using Swaply.Domain.Entities;
using Swaply.Domain.Exceptions;
using Swaply.Domain.Repositories;

namespace Swaply.Application.ExchangeManagement;

public class ExchangeService : IExchangeService
{
    private readonly IExchangeRepository _exchangeRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IExchangeDomainService _exchangeDomainService;

    public ExchangeService(
        IExchangeRepository exchangeRepository,
        IListingRepository listingRepository,
        IExchangeDomainService exchangeDomainService)
    {
        _exchangeRepository = exchangeRepository;
        _listingRepository = listingRepository;
        _exchangeDomainService = exchangeDomainService;
    }

    public async Task<Exchange> ProposeExchangeAsync(Guid proposerListingId, Guid receiverListingId, CancellationToken cancellationToken = default)
    {
        var proposerListing = await _listingRepository.GetByIdAsync(proposerListingId, cancellationToken)
            ?? throw new ListingNotFoundException(proposerListingId);
            
        var receiverListing = await _listingRepository.GetByIdAsync(receiverListingId, cancellationToken)
            ?? throw new ListingNotFoundException(receiverListingId);

        if (!_exchangeDomainService.CanPerformExchange(proposerListing, receiverListing))
        {
            throw new InvalidOperationException("Exchange cannot be performed. Listings must be active.");
        }

        var exchange = new Exchange(
            Guid.NewGuid(), 
            proposerListingId, 
            receiverListingId, 
            proposerListing.OwnerId, 
            receiverListing.OwnerId
        );

        await _exchangeRepository.AddAsync(exchange, cancellationToken);
        return exchange;
    }

    public async Task<bool> AcceptExchangeAsync(Guid exchangeId, CancellationToken cancellationToken = default)
    {
        var exchange = await _exchangeRepository.GetByIdAsync(exchangeId, cancellationToken);
        if (exchange == null) return false;

        exchange.Accept();
        
        var proposerListing = await _listingRepository.GetByIdAsync(exchange.ProposerListingId, cancellationToken);
        var receiverListing = await _listingRepository.GetByIdAsync(exchange.ReceiverListingId, cancellationToken);

        if (proposerListing != null && receiverListing != null)
        {
            proposerListing.MarkAsExchanged();
            receiverListing.MarkAsExchanged();

            await _listingRepository.UpdateAsync(proposerListing, cancellationToken);
            await _listingRepository.UpdateAsync(receiverListing, cancellationToken);
        }

        exchange.Complete();
        await _exchangeRepository.UpdateAsync(exchange, cancellationToken);
        
        return true;
    }

    public async Task<bool> RejectExchangeAsync(Guid exchangeId, CancellationToken cancellationToken = default)
    {
        var exchange = await _exchangeRepository.GetByIdAsync(exchangeId, cancellationToken);
        if (exchange == null) return false;

        exchange.Reject();
        await _exchangeRepository.UpdateAsync(exchange, cancellationToken);
        return true;
    }
}
