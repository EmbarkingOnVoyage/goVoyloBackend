using GoVoylo.Application.Features.Booking.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Booking.Commands
{
    
    public record BookFlightCommand(
    string FlightNumber,
    string PassengerName,
    string From,
    string To,
    DateTime JourneyDate,
    int NumberOfPassengers
) : IRequest<BookFlightResponseDto>;
}

