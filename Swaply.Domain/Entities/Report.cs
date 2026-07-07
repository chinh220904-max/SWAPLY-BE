namespace Swaply.Domain.Entities;

public class Report
{
    public Guid Id { get; private set; }
    public Guid ReporterId { get; private set; }
    public ReportTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public ReportReason Reason { get; private set; }
    public string? Description { get; private set; }
    public ReportStatus Status { get; private set; }
    public string? AdminNote { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    // Navigation property
    public User? Reporter { get; private set; }

    // EF Core constructor
    private Report() { }

    public Report(Guid reporterId, ReportTargetType targetType, Guid targetId, ReportReason reason, string? description = null)
    {
        Id = Guid.NewGuid();
        ReporterId = reporterId;
        TargetType = targetType;
        TargetId = targetId;
        Reason = reason;
        Description = description?.Trim();
        Status = ReportStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Approve(string adminNote)
    {
        Status = ReportStatus.Approved;
        AdminNote = adminNote?.Trim();
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject(string adminNote)
    {
        Status = ReportStatus.Rejected;
        AdminNote = adminNote?.Trim();
        ProcessedAt = DateTime.UtcNow;
    }
}
