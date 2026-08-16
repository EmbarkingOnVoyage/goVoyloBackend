using GoVoylo.Application.Features.Admin.Roles.Commands.CreateRole;
using GoVoylo.Application.Features.Admin.Roles.Commands.DeleteRole;
using GoVoylo.Application.Features.Admin.Roles.Commands.GrantRole;
using GoVoylo.Application.Features.Admin.Roles.Commands.RevokeRole;
using GoVoylo.Application.Features.Admin.Roles.Commands.UpdateRole;
using GoVoylo.Application.Features.Admin.Roles.Queries.GetRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoVoylo.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "superadmin")]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly ISender _mediator;

        public AdminController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var result = await _mediator.Send(new GetRolesQuery());
            return Ok(result);
        }

        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            var result = await _mediator.Send(new CreateRoleCommand(request.Name));
            return Ok(result);
        }

        [HttpPut("roles/{id}")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
        {
            var result = await _mediator.Send(new UpdateRoleCommand(id, request.Name));
            return Ok(result);
        }

        [HttpDelete("roles/{id}")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            await _mediator.Send(new DeleteRoleCommand(id));
            return Ok(new { message = "Role deleted successfully." });
        }

        [HttpPost("users/{userId}/roles")]
        public async Task<IActionResult> GrantRole(Guid userId, [FromBody] RoleAssignmentRequest request)
        {
            await _mediator.Send(new GrantRoleCommand(userId, request.RoleId));
            return Ok(new { message = "Role granted successfully." });
        }

        [HttpDelete("users/{userId}/roles/{roleId}")]
        public async Task<IActionResult> RevokeRole(Guid userId, Guid roleId)
        {
            await _mediator.Send(new RevokeRoleCommand(userId, roleId));
            return Ok(new { message = "Role revoked successfully." });
        }
    }

    public record CreateRoleRequest(string Name);
    public record UpdateRoleRequest(string Name);
    public record RoleAssignmentRequest(Guid RoleId);
}
