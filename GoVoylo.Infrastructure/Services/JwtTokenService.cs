using GoVoylo.Application.Interfaces;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using GoVoylo.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace GoVoylo.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public const string GuestRole = "guest";

        public string GenerateGuestToken(User guest, TimeSpan lifetime) =>
            Build(guest, new[] { GuestRole }, DateTime.UtcNow.Add(lifetime));

        public string GenerateToken(User user, IEnumerable<string> roles) =>
            Build(user, roles, DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpiryMinutes")));

        private string Build(User user, IEnumerable<string> roles, DateTime expires)
        {
            // Get JWT secret from .env
            var secretKey =
                Environment.GetEnvironmentVariable("JWT_SECRET");

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException(
                    "JWT_SECRET is not configured.");
            }

            // Get normal configuration from appsettings.json
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Email,
                    user.ContactEmail ?? string.Empty),

                new Claim(
                    ClaimTypes.Name,
                    $"{user.FirstName} {user.LastName}")
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey!));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}
