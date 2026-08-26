namespace GoVoylo.Application.Features.Customer.Dtos
{
    public record CustomerDashboardDto(
        CustomerProfileDto Profile,
        int SavedTravelerCount,
        int UpcomingBookingCount);
}
