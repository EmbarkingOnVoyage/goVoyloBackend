using GoVoylo.Application.Features.Flights.Dtos;
using GoVoylo.Domain.Entities;

namespace GoVoylo.Application.Features.Flights.Queries.GetMyTripBookings
{
    public static class TripBookingMapping
    {
        public static TripBookingDto ToDto(TripBooking b) => new(
            b.Id,
            b.SupplierCode,
            b.BookingRefNo,
            b.AirlinePnr,
            b.CrsPnr,
            b.StatusId,
            b.LocalStatus,
            b.TotalAmount,
            b.CurrencyCode,
            b.PassengerNames,
            b.CreatedAt,
            b.CancellationType,
            b.CancelCode,
            b.Legs
                .OrderBy(l => l.LegIndex)
                .Select(l => new TripBookingLegDto(
                    l.LegIndex,
                    l.Origin,
                    l.Destination,
                    l.TravelDate,
                    l.AirlineCode,
                    l.AirlineName,
                    l.FlightNumber,
                    l.AirlinePnr,
                    l.CrsPnr,
                    l.IsCancelled))
                .ToList(),
            b.CancelledAt,
            b.RefundAmount,
            b.ConvenienceFee);
    }
}
