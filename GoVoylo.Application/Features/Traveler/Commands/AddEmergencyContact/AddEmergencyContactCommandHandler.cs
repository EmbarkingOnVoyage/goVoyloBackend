using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Traveler.Dtos;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Traveler.Commands.AddEmergencyContact
{
    public class AddEmergencyContactCommandHandler
        : IRequestHandler<AddEmergencyContactCommand, EmergencyContactDto>
    {
        private readonly ISavedTravelerRepository _travelerRepository;
        private readonly ITravelerEmergencyContactRepository _contactRepository;

        public AddEmergencyContactCommandHandler(
            ISavedTravelerRepository travelerRepository,
            ITravelerEmergencyContactRepository contactRepository)
        {
            _travelerRepository = travelerRepository;
            _contactRepository = contactRepository;
        }

        public async Task<EmergencyContactDto> Handle(
            AddEmergencyContactCommand request, CancellationToken cancellationToken)
        {
            var traveler = await _travelerRepository.GetByIdAsync(request.TravelerId);

            if (traveler == null || traveler.UserId != request.UserId)
            {
                throw new NotFoundException("Traveler not found.");
            }

            var contact = new TravelerEmergencyContact(
                traveler.Id, request.Name, request.Relationship, request.Phone, request.PhoneCountryCode, request.Email);

            await _contactRepository.AddAsync(contact);

            return new EmergencyContactDto(
                contact.Id, contact.Name, contact.Relationship, contact.Phone, contact.PhoneCountryCode, contact.Email);
        }
    }
}
