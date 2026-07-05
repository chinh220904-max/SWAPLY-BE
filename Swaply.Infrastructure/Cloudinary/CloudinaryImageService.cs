using Swaply.Application.ListingManagement;

namespace Swaply.Infrastructure.Cloudinary;

public class CloudinaryImageService : IImageUploadService
{
    public Task<string> UploadImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        // Mock Cloudinary upload result
        var mockUrl = $"https://res.cloudinary.com/swaply/image/upload/v123456789/{Guid.NewGuid()}_{fileName}";
        Console.WriteLine($"[Cloudinary] Uploaded {fileName} successfully. URL: {mockUrl}");
        return Task.FromResult(mockUrl);
    }
}
