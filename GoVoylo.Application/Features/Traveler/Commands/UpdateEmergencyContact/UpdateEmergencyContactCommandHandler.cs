using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Traveler.Dtos;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.UpdateEmergencyContact
{
    public class UpdateEmergencyContactCommandHandler
        : IRequestHandler<UpdateEmergencyContactCommand, EmergencyContactDto>
    {
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly ITravelerEmergencyContactRepository _contactRepository;

        public UpdateEmergencyContactCommandHandler(
            ISavedTravelerRepository travelerRepository,
            ITravelerEmergencyContactRepository contactRepository)
        {
            _travelerRepository = travelerRepository;
            _contactRepository = contactRepository;
        }

        public async Task<EmergencyContactDto> Handle(
            UpdateEmergencyContactCommand request, CancellationToken cancellationToken)
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

            contact.Update(request.Name, request.Relationship, request.Phone, request.PhoneCountryCode, request.Email);
            await _contactRepository.UpdateAsync(contact);

            return new EmergencyContactDto(
                contact.Id, contact.Name, contact.Relationship, contact.Phone, contact.PhoneCountryCode, contact.Email);
        }
    }
}
