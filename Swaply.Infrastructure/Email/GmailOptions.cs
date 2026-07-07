namespace Swaply.Infrastructure.Email;

public class GmailOptions
{
    public const string SectionName = "Gmail";
    
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
    public string SenderName { get; set; } = "Swaply";
}
