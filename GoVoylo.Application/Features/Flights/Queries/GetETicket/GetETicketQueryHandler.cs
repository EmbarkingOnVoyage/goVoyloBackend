using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Interfaces;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Flights.Queries.GetETicket
{
    public class GetETicketQueryHandler : IRequestHandler<GetETicketQuery, ETicketFile>
    {
        // Air_Ticketing / Tripjack status for an issued ticket — only those have one.
        private const string StatusTicketed = "11";

        private readonly ITripBookingRepository _tripBookingRepository;
        private readonly IETicketService _eTicketService;

        public GetETicketQueryHandler(ITripBookingRepository tripBookingRepository, IETicketService eTicketService)
        {
            _tripBookingRepository = tripBookingRepository;
            _eTicketService = eTicketService;
        }

        public async Task<ETicketFile> Handle(GetETicketQuery request, CancellationToken cancellationToken)
        {
            var booking = await _tripBookingRepository.GetByIdAsync(request.TripBookingId, cancellationToken)
                ?? throw new NotFoundException("Booking not found.");

            if (booking.UserId != request.UserId)
            {
                throw new ForbiddenException("not_your_booking", "This booking does not belong to you.");
            }

            if (booking.StatusId != StatusTicketed || booking.LocalStatus != TripBooking.StatusActive)
            {
                throw new BusinessRuleException(
                    "no_eticket", "An e-ticket is only available once the booking is ticketed.");
            }

            return await _eTicketService.BuildPdfAsync(booking, cancellationToken);
        }
    }
}
