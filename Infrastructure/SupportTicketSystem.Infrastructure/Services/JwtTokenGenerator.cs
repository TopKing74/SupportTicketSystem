using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SupportTicketSystem.Application.Interfaces;
using SupportTicketSystem.Domain.Entities;
using static SupportTicketSystem.Application.DTOs.AuthDtos;

namespace SupportTicketSystem.Infrastructure.Services
{
    public class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
    {
        public AuthResponseDto GenerateTokens(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var keyDescription = configuration["Jwt:Key"] ?? "DefaultSecretKeyForLocalDevelopment1234567890!";
            var keyBytes = Encoding.UTF8.GetBytes(keyDescription);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var minutes = double.Parse(configuration["Jwt:DurationInMinutes"] ?? "60");
            var expirationTime = DateTime.UtcNow.AddMinutes(minutes);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                expires: expirationTime,
                claims: claims,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            var dummyRefreshToken = Guid.NewGuid().ToString(); 

            return new AuthResponseDto(tokenString, dummyRefreshToken, expirationTime, user.FullName, user.Email ?? string.Empty);
        }
    }
}
