using FluentValidation;

namespace GoVoylo.Application.Features.Customer.Commands.UploadProfileImage
{
    public class UploadProfileImageCommandValidator
        : AbstractValidator<UploadProfileImageCommand>
    {
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png" };
        private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB, per GV-CUST-BE-004

        public UploadProfileImageCommandValidator()
        {
            RuleFor(x => x.ContentType)
                .Must(ct => AllowedContentTypes.Contains(ct.ToLowerInvariant()))
                .WithMessage("Only JPG and PNG images are allowed.");

            RuleFor(x => x.FileSizeBytes)
                .LessThanOrEqualTo(MaxFileSizeBytes)
                .WithMessage("Image must be 2MB or smaller.");
        }
    }
}
