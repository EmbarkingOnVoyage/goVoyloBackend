using GoVoylo.Application.Features.Traveler.Commands.AddEmergencyContact;
using GoVoylo.Application.Features.Traveler.Commands.AddFrequentFlyer;
using GoVoylo.Application.Features.Traveler.Commands.AddPassport;
using GoVoylo.Application.Features.Traveler.Commands.AddTraveler;
using GoVoylo.Application.Features.Traveler.Commands.AddVisa;
using GoVoylo.Application.Features.Traveler.Commands.DeleteEmergencyContact;
using GoVoylo.Application.Features.Traveler.Commands.DeleteFrequentFlyer;
using GoVoylo.Application.Features.Traveler.Commands.DeletePassport;
using GoVoylo.Application.Features.Traveler.Commands.DeleteTraveler;
using GoVoylo.Application.Features.Traveler.Commands.DeleteVisa;
using GoVoylo.Application.Features.Traveler.Commands.UpdateEmergencyContact;
using GoVoylo.Application.Features.Traveler.Commands.UpdatePassport;
using GoVoylo.Application.Features.Traveler.Commands.UpdateTraveler;
using GoVoylo.Application.Features.Traveler.Commands.UpdateTravelerPreferences;
using GoVoylo.Application.Features.Traveler.Commands.UpdateVisa;
using GoVoylo.Application.Features.Traveler.Queries.GetTravelerById;
using GoVoylo.Application.Features.Traveler.Queries.GetTravelers;
using GoVoylo.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoVoylo.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/travellers")]
    public class TravellersController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly ICurrentUserService _currentUser;

        public TravellersController(ISender mediator, ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<IActionResult> GetTravellers()
        {
            var result = await _mediator.Send(new GetTravelersQuery(_currentUser.UserId));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTraveller(Guid id)
        {
            var result = await _mediator.Send(new GetTravelerByIdQuery(_currentUser.UserId, id));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddTraveller([FromBody] TravelerRequest request)
        {
            var command = new AddTravelerCommand(
                _currentUser.UserId,
                request.TravelerType,
                request.FirstName,
                request.LastName,
                request.DateOfBirth,
                request.Gender,
                request.Nationality,
                request.City,
                request.State,
                request.AutoAddTravelInsurance);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTraveller(Guid id, [FromBody] TravelerRequest request)
        {
            var command = new UpdateTravelerCommand(
                _currentUser.UserId,
                id,
                request.TravelerType,
                request.FirstName,
                request.LastName,
                request.DateOfBirth,
                request.Gender,
                request.Nationality,
                request.City,
                request.State,
                request.AutoAddTravelInsurance);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTraveller(Guid id)
        {
            await _mediator.Send(new DeleteTravelerCommand(_currentUser.UserId, id));
            return Ok(new { message = "Traveler deleted successfully." });
        }

        [HttpPost("{id}/passport")]
        public async Task<IActionResult> AddPassport(Guid id, [FromBody] PassportRequest request)
        {
            var command = new AddPassportCommand(
                _currentUser.UserId, id, request.PassportNumber, request.IssuingCountry, request.ExpiryDate);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}/passport")]
        public async Task<IActionResult> UpdatePassport(Guid id, [FromBody] PassportRequest request)
        {
            var command = new UpdatePassportCommand(
                _currentUser.UserId, id, request.PassportNumber, request.IssuingCountry, request.ExpiryDate);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}/passport")]
        public async Task<IActionResult> DeletePassport(Guid id)
        {
            await _mediator.Send(new DeletePassportCommand(_currentUser.UserId, id));
            return Ok(new { message = "Passport removed successfully." });
        }

        [HttpPost("{id}/visa")]
        public async Task<IActionResult> AddVisa(Guid id, [FromBody] VisaRequest request)
        {
            var command = new AddVisaCommand(
                _currentUser.UserId, id, request.Country, request.VisaNumber,
                request.VisaType, request.IssueDate, request.ExpiryDate);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}/visa/{visaId}")]
        public async Task<IActionResult> UpdateVisa(Guid id, Guid visaId, [FromBody] UpdateVisaRequest request)
        {
            var command = new UpdateVisaCommand(
                _currentUser.UserId, id, visaId, request.VisaNumber,
                request.VisaType, request.IssueDate, request.ExpiryDate);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}/visa/{visaId}")]
        public async Task<IActionResult> DeleteVisa(Guid id, Guid visaId)
        {
            await _mediator.Send(new DeleteVisaCommand(_currentUser.UserId, id, visaId));
            return Ok(new { message = "Visa removed successfully." });
        }

        [HttpPost("{id}/frequent-flyer")]
        public async Task<IActionResult> AddFrequentFlyer(Guid id, [FromBody] FrequentFlyerRequest request)
        {
            var command = new AddFrequentFlyerCommand(
                _currentUser.UserId, id, request.AirlineCode, request.MembershipNumber);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}/frequent-flyer/{frequentFlyerId}")]
        public async Task<IActionResult> DeleteFrequentFlyer(Guid id, Guid frequentFlyerId)
        {
            await _mediator.Send(new DeleteFrequentFlyerCommand(_currentUser.UserId, id, frequentFlyerId));
            return Ok(new { message = "Frequent flyer membership removed successfully." });
        }

        [HttpPut("{id}/preferences")]
        public async Task<IActionResult> UpdatePreferences(Guid id, [FromBody] TravelerPreferencesRequest request)
        {
            var command = new UpdateTravelerPreferencesCommand(
                _currentUser.UserId, id, request.MealPreference, request.SeatPreference, request.SpecialAssistance);

            await _mediator.Send(command);
            return Ok(new { message = "Preferences updated successfully." });
        }

        [HttpPost("{id}/emergency-contact")]
        public async Task<IActionResult> AddEmergencyContact(Guid id, [FromBody] EmergencyContactRequest request)
        {
            var command = new AddEmergencyContactCommand(
                _currentUser.UserId, id, request.Name, request.Relationship,
                request.Phone, request.PhoneCountryCode, request.Email);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}/emergency-contact/{contactId}")]
        public async Task<IActionResult> UpdateEmergencyContact(
            Guid id, Guid contactId, [FromBody] EmergencyContactRequest request)
        {
            var command = new UpdateEmergencyContactCommand(
                _currentUser.UserId, id, contactId, request.Name, request.Relationship,
                request.Phone, request.PhoneCountryCode, request.Email);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}/emergency-contact/{contactId}")]
        public async Task<IActionResult> DeleteEmergencyContact(Guid id, Guid contactId)
        {
            await _mediator.Send(new DeleteEmergencyContactCommand(_currentUser.UserId, id, contactId));
            return Ok(new { message = "Emergency contact removed successfully." });
        }
    }

    public record TravelerRequest(
        string TravelerType,
        string FirstName,
        string LastName,
        DateTime DateOfBirth,
        string? Gender,
        string? Nationality,
        string? City,
        string? State,
        bool AutoAddTravelInsurance);

    public record PassportRequest(string PassportNumber, string IssuingCountry, DateTime ExpiryDate);

    public record VisaRequest(
        string Country, string VisaNumber, string? VisaType, DateTime? IssueDate, DateTime ExpiryDate);

    public record UpdateVisaRequest(
        string VisaNumber, string? VisaType, DateTime? IssueDate, DateTime ExpiryDate);

    public record FrequentFlyerRequest(string AirlineCode, string MembershipNumber);

    public record TravelerPreferencesRequest(
        string? MealPreference, string? SeatPreference, IReadOnlyList<string> SpecialAssistance);

    public record EmergencyContactRequest(
        string Name, string? Relationship, string Phone, string PhoneCountryCode, string? Email);
}
