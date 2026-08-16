using GoVoylo.Application.Features.Admin.Roles.Commands.CreateRole;
using GoVoylo.Application.Features.Admin.Roles.Commands.DeleteRole;
using GoVoylo.Application.Features.Admin.Roles.Commands.GrantRole;
using GoVoylo.Application.Features.Admin.Roles.Commands.RevokeRole;
using GoVoylo.Application.Features.Admin.Roles.Commands.UpdateRole;
using GoVoylo.Application.Features.Admin.Roles.Queries.GetRoles;
using GoVoylo.Application.Features.Admin.Users.Commands.UpdateCustomerStatus;
using GoVoylo.Application.Features.Admin.Users.Queries.SearchUsers;
using GoVoylo.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoVoylo.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly ICurrentUserService _currentUser;

        public AdminController(ISender mediator, ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        [HttpGet("users")]
        [Authorize(Roles = "support_agent,superadmin")]
        public async Task<IActionResult> SearchUsers(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new SearchUsersQuery(search, status, page, pageSize));
            return Ok(result);
        }

        [HttpGet("roles")]
        [Authorize(Roles = "superadmin")]
        public async Task<IActionResult> GetRoles()
        {
            var result = await _mediator.Send(new GetRolesQuery());
            return Ok(result);
        }

        [HttpPost("roles")]
        [Authorize(Roles = "superadmin")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            var result = await _mediator.Send(new CreateRoleCommand(request.Name));
            return Ok(result);
        }

        [HttpPut("roles/{id}")]
        [Authorize(Roles = "superadmin")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
        {
            var result = await _mediator.Send(new UpdateRoleCommand(id, request.Name));
            return Ok(result);
        }

        [HttpDelete("roles/{id}")]
        [Authorize(Roles = "superadmin")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            await _mediator.Send(new DeleteRoleCommand(id));
            return Ok(new { message = "Role deleted successfully." });
        }

        [HttpPost("users/{userId}/roles")]
        [Authorize(Roles = "superadmin")]
        public async Task<IActionResult> GrantRole(Guid userId, [FromBody] RoleAssignmentRequest request)
        {
            await _mediator.Send(new GrantRoleCommand(userId, request.RoleId));
            return Ok(new { message = "Role granted successfully." });
        }

        [HttpDelete("users/{userId}/roles/{roleId}")]
        [Authorize(Roles = "superadmin")]
        public async Task<IActionResult> RevokeRole(Guid userId, Guid roleId)
        {
            await _mediator.Send(new RevokeRoleCommand(userId, roleId));
            return Ok(new { message = "Role revoked successfully." });
        }

        [HttpPut("users/{userId}/status")]
        [Authorize(Roles = "superadmin")]
        public async Task<IActionResult> UpdateCustomerStatus(Guid userId, [FromBody] UpdateCustomerStatusRequest request)
        {
            var command = new UpdateCustomerStatusCommand(_currentUser.UserId, userId, request.Status);
            await _mediator.Send(command);
            return Ok(new { message = "Account status updated successfully." });
        }
    }

    public record CreateRoleRequest(string Name);
    public record UpdateRoleRequest(string Name);
    public record RoleAssignmentRequest(Guid RoleId);
    public record UpdateCustomerStatusRequest(string Status);
}
