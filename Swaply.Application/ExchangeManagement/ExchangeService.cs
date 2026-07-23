using Microsoft.Extensions.Logging;
using Swaply.Application.NotificationManagement;
using Swaply.Domain.DomainServices;
using Swaply.Domain.Entities;
using Swaply.Domain.Enums;
using Swaply.Domain.Exceptions;
using Swaply.Domain.Repositories;

namespace Swaply.Application.ExchangeManagement;

public class ExchangeService : IExchangeService
{
    private readonly IExchangeRepository _exchangeRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IExchangeDomainService _exchangeDomainService;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ExchangeService> _logger;

    public ExchangeService(
        IExchangeRepository exchangeRepository,
        IListingRepository listingRepository,
        IConversationRepository conversationRepository,
        IExchangeDomainService exchangeDomainService,
        INotificationService notificationService,
        IUserRepository userRepository,
        ILogger<ExchangeService> logger)
    {
        _exchangeRepository = exchangeRepository;
        _listingRepository = listingRepository;
        _conversationRepository = conversationRepository;
        _exchangeDomainService = exchangeDomainService;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ExchangeDto> CreateExchangeAsync(CreateExchangeRequest request, Guid proposerId, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new InvalidExchangeStateException("Request cannot be null.");

        if (request.ProposerListingId == Guid.Empty || request.ReceiverListingId == Guid.Empty)
            throw new InvalidExchangeStateException("Listing ids are required.");

        var proposerListing = await _listingRepository.GetByIdAsync(request.ProposerListingId, cancellationToken);
        var receiverListing = await _listingRepository.GetByIdAsync(request.ReceiverListingId, cancellationToken);

        if (proposerListing == null)
            throw new ListingNotFoundException(request.ProposerListingId);

        if (receiverListing == null)
            throw new ListingNotFoundException(request.ReceiverListingId);

        if (proposerListing.IsDeleted)
            throw new InvalidExchangeStateException("Proposer listing is deleted.");

        if (receiverListing.IsDeleted)
            throw new InvalidExchangeStateException("Receiver listing is deleted.");

        if (proposerListing.OwnerId != proposerId)
            throw new UnauthorizedExchangeActionException("User does not own the proposer listing.");

        var receiverId = receiverListing.OwnerId;

        if (proposerId == receiverId)
            throw new InvalidExchangeStateException("Requester cannot exchange with themselves.");

        if (!_exchangeDomainService.CanPerformExchange(proposerListing, receiverListing))
            throw new InvalidExchangeStateException("Exchange cannot be performed. Both listings must be active.");

        var exchange = new Exchange(
            request.ProposerListingId,
            request.ReceiverListingId,
            proposerId,
            receiverId,
            request.Message
        );

        await _exchangeRepository.AddAsync(exchange, cancellationToken);

        var user1Id = proposerId.CompareTo(receiverId) < 0 ? proposerId : receiverId;
        var user2Id = proposerId.CompareTo(receiverId) < 0 ? receiverId : proposerId;
        var conversation = new Conversation(
            user1Id: user1Id,
            user2Id: user2Id,
            relatedListingId: request.ReceiverListingId,
            relatedExchangeId: exchange.Id
        );
        await _conversationRepository.AddAsync(conversation, cancellationToken);

        await _exchangeRepository.SaveChangesAsync(cancellationToken);

        var proposer = await _userRepository.GetByIdAsync(proposerId);
        var proposerName = proposer?.UserName ?? "A user";

        try
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest(
                UserId: receiverId,
                Title: "New Exchange Proposal",
                Content: $"{proposerName} has sent you an exchange proposal.",
                Type: "ExchangeProposal",
                RelatedEntityId: exchange.Id,
                RelatedEntityType: "Exchange"
            ), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Best-effort notification failed for Exchange {ExchangeId}, type {NotificationType}",
                exchange.Id, "ExchangeProposal");
        }

        return new ExchangeDto(
            exchange.Id,
            exchange.ProposerListingId,
            exchange.ReceiverListingId,
            exchange.ProposerId,
            exchange.ReceiverId,
            exchange.Status,
            exchange.Message,
            exchange.CreatedAt,
            exchange.UpdatedAt,
            conversation.Id,
            exchange.ProposerConfirmedComplete,
            exchange.ReceiverConfirmedComplete,
            exchange.ProposerConfirmedAt,
            exchange.ReceiverConfirmedAt,
            exchange.CompletedAt
        );
    }

    public async Task<ExchangeDto?> GetExchangeByIdAsync(Guid id, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var exchange = await _exchangeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Exchange with ID {id} was not found.");

        EnsureParticipant(exchange, requesterId);
        return await MapToDto(exchange, cancellationToken);
    }

    public async Task<IEnumerable<ExchangeDto>> GetMyExchangesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var exchanges = await _exchangeRepository.GetMyExchangesAsync(userId, cancellationToken);
        var result = new List<ExchangeDto>();
        foreach (var exchange in exchanges)
            result.Add(await MapToDto(exchange, cancellationToken));
        return result;
    }

    public async Task<IEnumerable<ExchangeDto>> GetOutgoingExchangesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var exchanges = await _exchangeRepository.GetOutgoingExchangesAsync(userId, cancellationToken);
        var result = new List<ExchangeDto>();
        foreach (var exchange in exchanges)
            result.Add(await MapToDto(exchange, cancellationToken));
        return result;
    }

    public async Task<IEnumerable<ExchangeDto>> GetIncomingExchangesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var exchanges = await _exchangeRepository.GetIncomingExchangesAsync(userId, cancellationToken);
        var result = new List<ExchangeDto>();
        foreach (var exchange in exchanges)
            result.Add(await MapToDto(exchange, cancellationToken));
        return result;
    }

    public async Task<ExchangeDto> AcceptExchangeAsync(Guid exchangeId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var exchange = await LoadExchangeAsync(exchangeId, cancellationToken);

        if (exchange.ReceiverId != requesterId)
            throw new UnauthorizedExchangeActionException("Only the receiver can accept this exchange.");

        if (exchange.Status != ExchangeStatus.Pending)
            throw new InvalidExchangeStateException($"Cannot accept an exchange in status '{exchange.Status}'.");

        exchange.Accept();
        await _exchangeRepository.UpdateAsync(exchange, cancellationToken);
        await _exchangeRepository.SaveChangesAsync(cancellationToken);

        try
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest(
                UserId: exchange.ProposerId,
                Title: "Exchange Accepted",
                Content: "Your exchange proposal has been accepted.",
                Type: "ExchangeAccepted",
                RelatedEntityId: exchange.Id,
                RelatedEntityType: "Exchange"
            ), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Best-effort notification failed for Exchange {ExchangeId}, type {NotificationType}",
                exchange.Id, "ExchangeAccepted");
        }

        return await MapToDto(exchange, cancellationToken);
    }

    public async Task<ExchangeDto> RejectExchangeAsync(Guid exchangeId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var exchange = await LoadExchangeAsync(exchangeId, cancellationToken);

        if (exchange.ReceiverId != requesterId)
            throw new UnauthorizedExchangeActionException("Only the receiver can reject this exchange.");

        if (exchange.Status != ExchangeStatus.Pending)
            throw new InvalidExchangeStateException($"Cannot reject an exchange in status '{exchange.Status}'.");

        exchange.Reject();
        await _exchangeRepository.UpdateAsync(exchange, cancellationToken);
        await _exchangeRepository.SaveChangesAsync(cancellationToken);

        try
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest(
                UserId: exchange.ProposerId,
                Title: "Exchange Rejected",
                Content: "Your exchange proposal has been rejected.",
                Type: "ExchangeRejected",
                RelatedEntityId: exchange.Id,
                RelatedEntityType: "Exchange"
            ), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Best-effort notification failed for Exchange {ExchangeId}, type {NotificationType}",
                exchange.Id, "ExchangeRejected");
        }

        return await MapToDto(exchange, cancellationToken);
    }

    public async Task<ExchangeDto> CancelExchangeAsync(Guid exchangeId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var exchange = await LoadExchangeAsync(exchangeId, cancellationToken);

        if (exchange.ProposerId != requesterId)
            throw new UnauthorizedExchangeActionException("Only the proposer can cancel this exchange.");

        if (exchange.Status == ExchangeStatus.Completed || exchange.Status == ExchangeStatus.Rejected || exchange.Status == ExchangeStatus.Cancelled)
            throw new InvalidExchangeStateException($"Cannot cancel an exchange in status '{exchange.Status}'.");

        exchange.Cancel();
        await _exchangeRepository.UpdateAsync(exchange, cancellationToken);
        await _exchangeRepository.SaveChangesAsync(cancellationToken);

        return await MapToDto(exchange, cancellationToken);
    }

    public async Task<ExchangeDto> CompleteExchangeAsync(Guid exchangeId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var exchange = await LoadExchangeAsync(exchangeId, cancellationToken);

        if (exchange.ProposerId != requesterId && exchange.ReceiverId != requesterId)
            throw new UnauthorizedExchangeActionException("Only participants can confirm this exchange.");

        if (exchange.Status != ExchangeStatus.Accepted)
            throw new InvalidOperationException($"Cannot confirm completion for an exchange in status '{exchange.Status}'. It must be accepted.");

        exchange.ConfirmCompletion(requesterId);
        await _exchangeRepository.UpdateAsync(exchange, cancellationToken);

        var isNowCompleted = exchange.Status == ExchangeStatus.Completed;

        if (isNowCompleted)
        {
            var proposerListing = await _listingRepository.GetByIdAsync(exchange.ProposerListingId, cancellationToken);
            var receiverListing = await _listingRepository.GetByIdAsync(exchange.ReceiverListingId, cancellationToken);

            if (proposerListing != null && !proposerListing.IsDeleted)
                proposerListing.MarkAsExchanged();
            if (receiverListing != null && !receiverListing.IsDeleted)
                receiverListing.MarkAsExchanged();

            if (proposerListing != null)
                await _listingRepository.UpdateAsync(proposerListing, cancellationToken);
            if (receiverListing != null)
                await _listingRepository.UpdateAsync(receiverListing, cancellationToken);
        }

        await _exchangeRepository.SaveChangesAsync(cancellationToken);

        if (!isNowCompleted)
        {
            var otherUserId = exchange.ProposerId == requesterId ? exchange.ReceiverId : exchange.ProposerId;
            try
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationRequest(
                    UserId: otherUserId,
                    Title: "Exchange Confirmation Pending",
                    Content: "The other user has confirmed the exchange. Please confirm once you have completed the exchange.",
                    Type: "ExchangeConfirmationPending",
                    RelatedEntityId: exchange.Id,
                    RelatedEntityType: "Exchange"
                ), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Best-effort notification failed for Exchange {ExchangeId}, type {NotificationType}",
                    exchange.Id, "ExchangeConfirmationPending");
            }
        }
        else
        {
            try
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationRequest(
                    UserId: exchange.ProposerId,
                    Title: "Exchange Completed",
                    Content: "Your exchange has been completed successfully.",
                    Type: "ExchangeCompleted",
                    RelatedEntityId: exchange.Id,
                    RelatedEntityType: "Exchange"
                ), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Best-effort notification failed for Exchange {ExchangeId}, type {NotificationType}",
                    exchange.Id, "ExchangeCompleted");
            }

            try
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationRequest(
                    UserId: exchange.ReceiverId,
                    Title: "Exchange Completed",
                    Content: "Your exchange has been completed successfully.",
                    Type: "ExchangeCompleted",
                    RelatedEntityId: exchange.Id,
                    RelatedEntityType: "Exchange"
                ), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Best-effort notification failed for Exchange {ExchangeId}, type {NotificationType}",
                    exchange.Id, "ExchangeCompleted");
            }
        }

        return await MapToDto(exchange, cancellationToken);
    }

    private async Task<Exchange> LoadExchangeAsync(Guid exchangeId, CancellationToken cancellationToken)
    {
        var exchange = await _exchangeRepository.GetByIdAsync(exchangeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Exchange with ID {exchangeId} was not found.");
        return exchange;
    }

    private static void EnsureParticipant(Exchange exchange, Guid userId)
    {
        if (exchange.ProposerId != userId && exchange.ReceiverId != userId)
            throw new UnauthorizedExchangeActionException("User is not a participant of this exchange.");
    }

    private async Task<ExchangeDto> MapToDto(Exchange exchange, CancellationToken cancellationToken = default)
    {
        var conversationId = (await _conversationRepository.GetByExchangeIdAsync(exchange.Id, cancellationToken))?.Id;
        return new ExchangeDto(
            exchange.Id,
            exchange.ProposerListingId,
            exchange.ReceiverListingId,
            exchange.ProposerId,
            exchange.ReceiverId,
            exchange.Status,
            exchange.Message,
            exchange.CreatedAt,
            exchange.UpdatedAt,
            conversationId,
            exchange.ProposerConfirmedComplete,
            exchange.ReceiverConfirmedComplete,
            exchange.ProposerConfirmedAt,
            exchange.ReceiverConfirmedAt,
            exchange.CompletedAt
        );
    }
}
