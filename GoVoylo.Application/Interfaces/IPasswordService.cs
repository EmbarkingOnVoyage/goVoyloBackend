using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Application.Interfaces
{
    public interface IPasswordService
    {
        string HashPassword(string password);

        bool VerifyPassword(
            string password,
            string passwordHash);
    }
}
