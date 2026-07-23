using Swaply.Domain.Entities;
using Swaply.Domain.Enums;

namespace Swaply.Application.ExchangeManagement;

public record CreateExchangeRequest(
    Guid ProposerListingId,
    Guid ReceiverListingId,
    string? Message = null
);

public record UpdateExchangeStatusRequest(
    string? Message = null
);

public record ExchangeDto(
    Guid Id,
    Guid ProposerListingId,
    Guid ReceiverListingId,
    Guid ProposerId,
    Guid ReceiverId,
    ExchangeStatus Status,
    string? Message,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    Guid? ConversationId,
    bool ProposerConfirmedComplete,
    bool ReceiverConfirmedComplete,
    DateTime? ProposerConfirmedAt,
    DateTime? ReceiverConfirmedAt,
    DateTime? CompletedAt
);
