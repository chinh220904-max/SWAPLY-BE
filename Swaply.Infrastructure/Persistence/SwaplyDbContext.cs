using Swaply.Domain.Entities;
using System.Collections.Concurrent;

namespace Swaply.Infrastructure.Persistence;

public class SwaplyDbContext
{
    public ConcurrentBag<Listing> Listings { get; } = new();
    public ConcurrentBag<Exchange> Exchanges { get; } = new();

    public SwaplyDbContext()
    {
        // Seed some initial data
        SeedData();
    }

    private void SeedData()
    {
        var price1 = new Swaply.Domain.ValueObjects.Money(150000, "VND");
        var price2 = new Swaply.Domain.ValueObjects.Money(200000, "VND");

        Listings.Add(new Listing(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Cũi em bé gỗ sồi",
            "Cần đổi lấy xe đẩy em bé cũ. Cũi còn mới 90%.",
            "user_proposer",
            price1
        ));

        Listings.Add(new Listing(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Xe đẩy em bé Aprica",
            "Muốn đổi lấy cũi gỗ hoặc đồ chơi lắp ráp cho bé.",
            "user_receiver",
            price2
        ));
    }
}
