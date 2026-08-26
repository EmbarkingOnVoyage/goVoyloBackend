using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.DeleteEmergencyContact
{
    public class DeleteEmergencyContactCommandHandler : IRequestHandler<DeleteEmergencyContactCommand, Unit>
    {
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly ITravelerEmergencyContactRepository _contactRepository;

        public DeleteEmergencyContactCommandHandler(
            ISavedTravelerRepository travelerRepository,
            ITravelerEmergencyContactRepository contactRepository)
        {
            _travelerRepository = travelerRepository;
            _contactRepository = contactRepository;
        }

        public async Task<Unit> Handle(DeleteEmergencyContactCommand request, CancellationToken cancellationToken)
        {
            var traveler = await _travelerRepository.GetByIdAsync(request.TravelerId);

            if (traveler == null || traveler.UserId != request.UserId)
            {
                throw new NotFoundException("Traveler not found.");
            }

            var contact = await _contactRepository.GetByIdAsync(request.ContactId);

            if (contact == null || contact.SavedTravelerId != request.TravelerId)
            {
                throw new NotFoundException("Emergency contact not found.");
            }

            await _contactRepository.DeleteAsync(contact);
            return Unit.Value;
        }
    }
}
