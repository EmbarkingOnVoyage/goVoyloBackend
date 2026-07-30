using GoVoylo.Application.Features.Booking.Dtos;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Booking.Commands
{
    public class BookFlightCommandHandler
    : IRequestHandler<BookFlightCommand, BookFlightResponseDto>
    {
        private readonly IBookFlightRepository _bookingRepository;

        public BookFlightCommandHandler(
            IBookFlightRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<BookFlightResponseDto> Handle(
            BookFlightCommand request,
            CancellationToken cancellationToken)
        {

            if (request.NumberOfPassengers > 1)
            {
                throw new InvalidOperationException(
                    "Passenger details must be provided for all passengers.");
            }

            var booking = new FlightBooking(
                request.FlightNumber,
                request.PassengerName,
                request.From,
                request.To,
                request.JourneyDate,
                request.NumberOfPassengers
            );

            await _bookingRepository.SaveAsync(
                booking,
                cancellationToken);

            return new BookFlightResponseDto(
                booking.FlightBookingId,
                booking.FlightBookingReference,
                booking.FlightNumber,
                booking.PassengerName,
                booking.FlightBookingStatus,
                booking.BookedAt
            );
        }
    }
}
