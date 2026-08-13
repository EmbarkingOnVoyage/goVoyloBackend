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
            var extension = Path.GetExtension(fileName);
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
    }
}
