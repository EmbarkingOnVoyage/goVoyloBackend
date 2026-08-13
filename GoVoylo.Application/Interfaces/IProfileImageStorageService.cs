namespace GoVoylo.Application.Interfaces
{
    public interface IProfileImageStorageService
    {
        Task<string> UploadAsync(
            Guid userId,
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default);
    }
}
