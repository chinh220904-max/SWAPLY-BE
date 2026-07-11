using Microsoft.AspNetCore.Http;

namespace Swaply.Application.ListingManagement;

public interface IImageUploadService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<string> UploadFromFormFileAsync(IFormFile file, CancellationToken cancellationToken = default);
}
