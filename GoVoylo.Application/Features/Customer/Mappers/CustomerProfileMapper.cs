using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Domain.Entities;

namespace GoVoylo.Application.Features.Customer.Mappers
{
    public static class CustomerProfileMapper
    {
        public static CustomerProfileDto ToDto(User user)
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
                user.CreatedAt);
        }

        private static int CalculateCompletionPercentage(User user)
        {
            var fields = new[]
            {
                !string.IsNullOrWhiteSpace(user.Email),
                !string.IsNullOrWhiteSpace(user.Phone),
                user.IsEmailVerified,
                user.IsPhoneVerified,
                !string.IsNullOrWhiteSpace(user.ProfileImageUrl)
            };

            var completed = fields.Count(f => f);
            return (int)Math.Round(completed * 100d / fields.Length);
        }
    }
}
