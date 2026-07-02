using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupportTicketSystem.Domain.Entities;
using static SupportTicketSystem.Application.DTOs.AuthDtos;

namespace SupportTicketSystem.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        AuthResponseDto GenerateTokens(ApplicationUser user, IList<string> roles);
    }
}
