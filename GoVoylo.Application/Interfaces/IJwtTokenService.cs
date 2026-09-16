using GoVoylo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user, IEnumerable<string> roles);
    }
}
