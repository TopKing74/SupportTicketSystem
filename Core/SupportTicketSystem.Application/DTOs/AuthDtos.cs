using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTicketSystem.Application.DTOs
{
    public class AuthDtos
    {
        public record RegisterDto(string FullName, string Email, string Password, string ContactNumber);
        public record LoginDto(string Email, string Password);
        public record AuthResponseDto(string Token, string RefreshToken, DateTime ExpiresAt, string FullName, string Email);
    }
}
