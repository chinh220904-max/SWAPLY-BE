namespace Swaply.Domain.Entities;

public class UserMonthlyQuota
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public int TotalQuota { get; private set; }
    public int UsedQuota { get; private set; }

    private static readonly int DefaultQuota = 3;

    public User? User { get; private set; }

    private UserMonthlyQuota() { }

    public UserMonthlyQuota(Guid userId, int year, int month)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Year = year;
        Month = month;
        TotalQuota = DefaultQuota;
        UsedQuota = 0;
    }

    public static int GetCurrentYear() => DateTime.UtcNow.Year;
    public static int GetCurrentMonth() => DateTime.UtcNow.Month;

    public static UserMonthlyQuota CreateForCurrentMonth(Guid userId)
    {
        var now = DateTime.UtcNow;
        return new UserMonthlyQuota(userId, now.Year, now.Month);
    }

    public bool IsForCurrentPeriod()
    {
        var now = DateTime.UtcNow;
        return Year == now.Year && Month == now.Month;
    }

    public int RemainingQuota()
    {
        return TotalQuota - UsedQuota;
    }

    public bool CanUseQuota()
    {
        return RemainingQuota() > 0;
    }

    public bool UseQuota(int amount = 1)
    {
        if (!CanUseQuota())
            return false;

        UsedQuota += amount;
        return true;
    }

    public void Reset()
    {
        UsedQuota = 0;
    }
}
