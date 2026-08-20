using GoVoylo.Application.Features.Customer.Commands.AddCustomerAddress;
using GoVoylo.Application.Features.Customer.Commands.AddGstDetails;
using GoVoylo.Application.Features.Customer.Commands.ChangePassword;
using GoVoylo.Application.Features.Customer.Commands.DeleteCustomerAccount;
using GoVoylo.Application.Features.Customer.Commands.DeleteCustomerAddress;
using GoVoylo.Application.Features.Customer.Commands.DeleteProfileImage;
using GoVoylo.Application.Features.Customer.Commands.UpdateCustomerAddress;
using GoVoylo.Application.Features.Customer.Commands.UpdateCustomerProfile;
using GoVoylo.Application.Features.Customer.Commands.UpdateExtendedProfile;
using GoVoylo.Application.Features.Customer.Commands.UpdateGstDetails;
using GoVoylo.Application.Features.Customer.Commands.UpdateNotificationPreferences;
using GoVoylo.Application.Features.Customer.Commands.UpdatePreferences;
using GoVoylo.Application.Features.Customer.Commands.UploadProfileImage;
using GoVoylo.Application.Features.Customer.Queries.GetCustomerActivity;
using GoVoylo.Application.Features.Customer.Queries.GetCustomerAddresses;
using GoVoylo.Application.Features.Customer.Queries.GetCustomerDashboard;
using GoVoylo.Application.Features.Customer.Queries.GetCustomerFullProfile;
using GoVoylo.Application.Features.Customer.Queries.GetCustomerProfile;
using GoVoylo.Application.Features.Customer.Queries.GetGstDetails;
using GoVoylo.Application.Features.Customer.Queries.GetNotificationPreferences;
using GoVoylo.Application.Features.Customer.Queries.GetPreferences;
using GoVoylo.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoVoylo.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly ICurrentUserService _currentUser;

        public CustomerController(ISender mediator, ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _mediator.Send(new GetCustomerProfileQuery(_currentUser.UserId));
            return Ok(result);
        }

        [HttpGet("full-profile")]
        public async Task<IActionResult> GetFullProfile()
        {
            var result = await _mediator.Send(new GetCustomerFullProfileQuery(_currentUser.UserId));
            return Ok(result);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var command = new UpdateCustomerProfileCommand(
                _currentUser.UserId, request.FirstName, request.LastName, request.Phone);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("profile/details")]
        public async Task<IActionResult> UpdateExtendedProfile([FromBody] UpdateExtendedProfileRequest request)
        {
            var command = new UpdateExtendedProfileCommand(
                _currentUser.UserId,
                request.Gender,
                request.DateOfBirth,
                request.Nationality,
                request.MaritalStatus,
                request.Anniversary,
                request.CityOfResidence,
                request.State,
                request.PassportNumber,
                request.PassportExpiryDate,
                request.PassportIssuingCountry,
                request.PanCardNumber,
                request.AutoAddTravelInsurance);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var command = new ChangePasswordCommand(
                _currentUser.UserId, request.CurrentPassword, request.NewPassword);

            await _mediator.Send(command);
            return Ok(new { message = "Password updated successfully." });
        }

        [HttpPost("profile/image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            var command = new UploadProfileImageCommand(
                _currentUser.UserId, stream, file.FileName, file.ContentType, file.Length);

            var imageUrl = await _mediator.Send(command);
            return Ok(new { imageUrl });
        }

        [HttpDelete("profile/image")]
        public async Task<IActionResult> DeleteProfileImage()
        {
            await _mediator.Send(new DeleteProfileImageCommand(_currentUser.UserId));
            return Ok(new { message = "Image removed successfully." });
        }

        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount()
        {
            await _mediator.Send(new DeleteCustomerAccountCommand(_currentUser.UserId));
            return Ok(new { message = "Account deleted successfully." });
        }

        [HttpGet("activity")]
        public async Task<IActionResult> GetActivity()
        {
            var result = await _mediator.Send(new GetCustomerActivityQuery(_currentUser.UserId));
            return Ok(result);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _mediator.Send(new GetCustomerDashboardQuery(_currentUser.UserId));
            return Ok(result);
        }

        [HttpGet("preferences")]
        public async Task<IActionResult> GetPreferences()
        {
            var result = await _mediator.Send(new GetPreferencesQuery(_currentUser.UserId));
            return Ok(result);
        }

        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
        {
            var command = new UpdatePreferencesCommand(_currentUser.UserId, request.Language, request.Currency);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("notification-preferences")]
        public async Task<IActionResult> GetNotificationPreferences()
        {
            var result = await _mediator.Send(new GetNotificationPreferencesQuery(_currentUser.UserId));
            return Ok(result);
        }

        [HttpPut("notification-preferences")]
        public async Task<IActionResult> UpdateNotificationPreferences(
            [FromBody] UpdateNotificationPreferencesRequest request)
        {
            var command = new UpdateNotificationPreferencesCommand(
                _currentUser.UserId,
                request.EmailMarketing,
                request.SmsTransactional,
                request.SmsMarketing,
                request.PushEnabled);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("address")]
        public async Task<IActionResult> GetAddresses()
        {
            var result = await _mediator.Send(new GetCustomerAddressesQuery(_currentUser.UserId));
            return Ok(result);
        }

        [HttpPost("address")]
        public async Task<IActionResult> AddAddress([FromBody] AddressRequest request)
        {
            var command = new AddCustomerAddressCommand(
                _currentUser.UserId,
                request.Label,
                request.Line1,
                request.Line2,
                request.City,
                request.State,
                request.PostalCode,
                request.Country,
                request.IsDefault);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("address/{id}")]
        public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] AddressRequest request)
        {
            var command = new UpdateCustomerAddressCommand(
                _currentUser.UserId,
                id,
                request.Label,
                request.Line1,
                request.Line2,
                request.City,
                request.State,
                request.PostalCode,
                request.Country,
                request.IsDefault);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("address/{id}")]
        public async Task<IActionResult> DeleteAddress(Guid id)
        {
            await _mediator.Send(new DeleteCustomerAddressCommand(_currentUser.UserId, id));
            return Ok(new { message = "Address removed successfully." });
        }

        [HttpGet("gst")]
        public async Task<IActionResult> GetGstDetails()
        {
            var result = await _mediator.Send(new GetGstDetailsQuery(_currentUser.UserId));
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("gst")]
        public async Task<IActionResult> AddGstDetails([FromBody] GstDetailsRequest request)
        {
            var command = new AddGstDetailsCommand(
                _currentUser.UserId, request.Gstin, request.LegalName, request.TradeName);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("gst")]
        public async Task<IActionResult> UpdateGstDetails([FromBody] GstDetailsRequest request)
        {
            var command = new UpdateGstDetailsCommand(
                _currentUser.UserId, request.Gstin, request.LegalName, request.TradeName);

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }

    public record UpdateProfileRequest(string FirstName, string LastName, string? Phone);

    public record UpdateExtendedProfileRequest(
        string? Gender,
        DateTime? DateOfBirth,
        string? Nationality,
        string? MaritalStatus,
        DateTime? Anniversary,
        string? CityOfResidence,
        string? State,
        string? PassportNumber,
        DateTime? PassportExpiryDate,
        string? PassportIssuingCountry,
        string? PanCardNumber,
        bool AutoAddTravelInsurance);
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
    public record UpdatePreferencesRequest(string Language, string Currency);

    public record UpdateNotificationPreferencesRequest(
        bool EmailMarketing, bool SmsTransactional, bool SmsMarketing, bool PushEnabled);

    public record AddressRequest(
        string? Label,
        string Line1,
        string? Line2,
        string City,
        string State,
        string PostalCode,
        string Country,
        bool IsDefault);

    public record GstDetailsRequest(string Gstin, string LegalName, string? TradeName);
}
