using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UploadProfileImage
{
    public record UploadProfileImageCommand(
        Guid UserId,
        Stream FileStream,
        string FileName,
        string ContentType,
        long FileSizeBytes) : IRequest<string>;
}
