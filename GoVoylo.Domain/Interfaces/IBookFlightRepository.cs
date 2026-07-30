using GoVoylo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Interfaces
{
    public interface IBookFlightRepository
    {
        //Task SaveAsync(
        //FlightBooking flightbooking);

        Task<FlightBooking?> GetByBookingReferenceAsync(
            string bookingReference,
            CancellationToken cancellationToken);

        Task UpdateAsync(
            FlightBooking booking,
            CancellationToken cancellationToken);
        Task SaveAsync(FlightBooking booking, CancellationToken cancellationToken);
    }
}
