namespace Swaply.Application.ListingManagement;

public interface IImageUploadService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}
