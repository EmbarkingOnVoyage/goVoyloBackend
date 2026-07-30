using GoVoylo.Application.Features.Booking.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Booking.Query
{
    public record GetBookFlightQuery(
        string BookingReference
    ) : IRequest<GetBookFlightResponseDto>;
}
