namespace Swaply.Domain.Entities;

public class BoostPackageGoldenHour
{
    public Guid Id { get; private set; }
    public Guid BoostPackageId { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    public BoostPackage? BoostPackage { get; private set; }

    private BoostPackageGoldenHour() { }

    public BoostPackageGoldenHour(Guid boostPackageId, TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("StartTime must be before EndTime.");

        Id = Guid.NewGuid();
        BoostPackageId = boostPackageId;
        StartTime = startTime;
        EndTime = endTime;
    }

    public bool IsActiveNow()
    {
        var now = TimeOnly.FromDateTime(DateTime.UtcNow);
        return now >= StartTime && now < EndTime;
    }

    public bool IsActiveAt(TimeOnly time)
    {
        return time >= StartTime && time < EndTime;
    }
}
