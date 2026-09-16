using GoVoylo.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GoVoylo.Infrastructure.Services
{
    // Placeholder for the Azure Blob Storage adapter named in the architecture doc.
    // Stores files on local disk behind the same IProfileImageStorageService contract,
    // so swapping in the real Azure adapter later is a one-file change.
    public class LocalProfileImageStorageService : IProfileImageStorageService
    {
        private readonly string _rootPath;
        private readonly string _publicBasePath;

        public LocalProfileImageStorageService(IConfiguration configuration)
        {
            _rootPath = configuration["Storage:ProfileImages:RootPath"]
                ?? Path.Combine(AppContext.BaseDirectory, "uploads", "profile-images");
            _publicBasePath = configuration["Storage:ProfileImages:PublicBasePath"]
                ?? "/uploads/profile-images";

            Directory.CreateDirectory(_rootPath);
        }

        public async Task<string> UploadAsync(
            Guid userId,
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            // Extension is derived from the validated content type, never from the
            // client-supplied file name — otherwise a request with an allowed
            // Content-Type but an attacker-chosen file name (e.g. "x.svg", "x.html")
            // would be stored with that extension, and a browser navigating straight
            // to the file's public URL would render it as that type instead of an image.
            var extension = GetExtensionForContentType(contentType);
            var storedFileName = $"{userId}{extension}";
            var fullPath = Path.Combine(_rootPath, storedFileName);

            await using (var output = File.Create(fullPath))
            {
                await fileStream.CopyToAsync(output, cancellationToken);
            }

            return $"{_publicBasePath}/{storedFileName}";
        }

        public Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default)
        {
            var fileName = Path.GetFileName(imageUrl);
            var fullPath = Path.Combine(_rootPath, fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }

        private static string GetExtensionForContentType(string contentType) =>
            contentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                _ => throw new ArgumentException($"Unsupported content type: {contentType}", nameof(contentType))
            };
    }
}
