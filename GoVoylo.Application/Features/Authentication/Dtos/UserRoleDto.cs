using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Features.Authentication.Dtos
{
    public record UserRoleDto
    {
        public Guid RoleId { get; set; }

        public string RoleName { get; set; } = null!;
    }
}
