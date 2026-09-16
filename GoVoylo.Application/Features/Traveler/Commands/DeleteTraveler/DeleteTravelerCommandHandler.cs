using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.DeleteTraveler
{
    // Ticket GV-TRV-BE-006 also requires blocking deletion when a future booking
    // exists — Bookings doesn't exist in this codebase yet, so that check is a no-op.
    public class DeleteTravelerCommandHandler : IRequestHandler<DeleteTravelerCommand, Unit>
    {
        private readonly ISavedTravelerRepository _travelerRepository;

        public DeleteTravelerCommandHandler(ISavedTravelerRepository travelerRepository)
        {
            _travelerRepository = travelerRepository;
        }

        public async Task<Unit> Handle(DeleteTravelerCommand request, CancellationToken cancellationToken)
        {
            var traveler = await _travelerRepository.GetByIdAsync(request.TravelerId);

            if (traveler == null || traveler.UserId != request.UserId)
            {
                throw new NotFoundException("Traveler not found.");
            }

            traveler.SoftDelete();
            await _travelerRepository.UpdateAsync(traveler);

            return Unit.Value;
        }
    }
}
