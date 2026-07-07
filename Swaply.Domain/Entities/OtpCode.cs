namespace Swaply.Domain.Entities;

public class OtpCode
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public OtpType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }

    private OtpCode() { }

    public OtpCode(string email, string code, OtpType type, int expirationMinutes = 5)
    {
        Id = Guid.NewGuid();
        Email = email.ToLowerInvariant().Trim();
        Code = code;
        Type = type;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = CreatedAt.AddMinutes(expirationMinutes);
        IsUsed = false;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;

    public void MarkAsUsed()
    {
        IsUsed = true;
    }

    public bool Verify(string code)
    {
        return IsValid && Code == code;
    }
}

public enum OtpType
{
    EmailVerification,
    PasswordReset
}
