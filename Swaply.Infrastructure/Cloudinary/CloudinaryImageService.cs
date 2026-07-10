using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using Swaply.Application.ListingManagement;

namespace Swaply.Infrastructure.Cloudinary;

public class CloudinaryImageService : IImageUploadService
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    private readonly string _folder;

    public CloudinaryImageService(IOptions<CloudinaryOptions> options)
    {
        var opt = options.Value;
        _folder = opt.Folder;

        var account = new Account(opt.CloudName, opt.ApiKey, opt.ApiSecret);
        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = _folder,
            UseFilename = false,
            UniqueFilename = true,
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (uploadResult.Error != null)
            throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");

        return uploadResult.SecureUrl.ToString();
    }
}
