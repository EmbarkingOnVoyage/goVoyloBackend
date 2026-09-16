using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.DeletePassport
{
    // Ticket GV-TRV-BE-009 also requires blocking deletion when an active
    // international booking exists — Bookings doesn't exist yet, so this is a no-op.
    public class DeletePassportCommandHandler : IRequestHandler<DeletePassportCommand, Unit>
    {
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly ITravelerPassportRepository _passportRepository;

        public DeletePassportCommandHandler(
            ISavedTravelerRepository travelerRepository,
            ITravelerPassportRepository passportRepository)
        {
            _travelerRepository = travelerRepository;
            _passportRepository = passportRepository;
        }

        public async Task<Unit> Handle(DeletePassportCommand request, CancellationToken cancellationToken)
        {
            var traveler = await _travelerRepository.GetByIdAsync(request.TravelerId);

            if (traveler == null || traveler.UserId != request.UserId)
            {
                throw new NotFoundException("Traveler not found.");
            }

            var passport = await _passportRepository.GetByTravelerIdAsync(traveler.Id);

            if (passport != null)
            {
                await _passportRepository.DeleteAsync(passport);
            }

            return Unit.Value;
        }
    }
}
