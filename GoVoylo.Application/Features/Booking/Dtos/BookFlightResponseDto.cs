using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Booking.Dtos
{
    public record BookFlightResponseDto(
    Guid FlightBookingId,
    string FlightBookingReference,
    string FlightNumber,
    string PassengerName,
    string FlightBookingStatus,
    DateTime BookedAt
    );
}
