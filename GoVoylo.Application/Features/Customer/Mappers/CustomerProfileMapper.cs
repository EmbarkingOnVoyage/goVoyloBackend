using GoVoylo.Application.Common;
using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;

namespace GoVoylo.Application.Features.Customer.Mappers
{
    public static class CustomerProfileMapper
    {
        public static CustomerProfileDto ToDto(User user, IEncryptionService encryptionService)
        {
            return new CustomerProfileDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.IsEmailVerified,
                user.IsPhoneVerified,
                user.ProfileImageUrl,
                user.Status,
                CalculateCompletionPercentage(user),
                user.CreatedAt,
                user.Gender,
                user.DateOfBirth,
                user.Nationality,
                user.MaritalStatus,
                user.Anniversary,
                user.CityOfResidence,
                user.State,
                user.PassportNumberEncrypted == null
                    ? null
                    : MaskingHelper.MaskKeepLast4(encryptionService.Decrypt(user.PassportNumberEncrypted)),
                user.PassportExpiryDate,
                user.PassportIssuingCountry,
                user.PanCardNumberEncrypted == null
                    ? null
                    : MaskingHelper.MaskKeepLast4(encryptionService.Decrypt(user.PanCardNumberEncrypted)),
                user.AutoAddTravelInsurance);
        }

        private static int CalculateCompletionPercentage(User user)
        {
            var fields = new[]
            {
                !string.IsNullOrWhiteSpace(user.Email),
                !string.IsNullOrWhiteSpace(user.Phone),
                user.IsEmailVerified,
                user.IsPhoneVerified,
                !string.IsNullOrWhiteSpace(user.ProfileImageUrl),
                !string.IsNullOrWhiteSpace(user.Gender),
                user.DateOfBirth.HasValue,
                !string.IsNullOrWhiteSpace(user.Nationality),
                !string.IsNullOrWhiteSpace(user.MaritalStatus),
                !string.IsNullOrWhiteSpace(user.CityOfResidence),
                !string.IsNullOrWhiteSpace(user.State),
                user.PassportNumberEncrypted != null
            };

            var completed = fields.Count(f => f);
            return (int)Math.Round(completed * 100d / fields.Length);
        }
    }
}
