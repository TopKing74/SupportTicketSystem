using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupportTicketSystem.Domain.Enums;
using static SupportTicketSystem.Application.DTOs.ReplyDtos;
using static SupportTicketSystem.Application.DTOs.TicketDtos;

namespace SupportTicketSystem.Application.Interfaces
{
    public record DashboardCountsDto(int Open, int InProgress, int Resolved, int Closed, int Total);

    public interface ITicketService
    {
        Task<TicketDetailsDto> CreateTicketAsync(CreateTicketDto dto);
        Task<IEnumerable<TicketSummaryDto>> GetMyTicketsAsync();
        Task<IEnumerable<TicketSummaryDto>> GetAssignedTicketsAsync();
        Task<IEnumerable<TicketSummaryDto>> SearchTicketsAsync(TicketSearchQuery query);
        Task<TicketDetailsDto?> GetTicketByIdAsync(Guid id);
        Task<bool> UpdateTicketStatusAsync(Guid id, TicketStatus newStatus);
        Task<bool> AssignAgentAsync(Guid id, string agentId);
        Task<ReplyDto> ReplyToTicketAsync(Guid id, CreateReplyDto dto);
        Task<IEnumerable<ReplyDto>> GetTicketRepliesAsync(Guid id);
        Task<DashboardCountsDto> GetDashboardCountsAsync();
    }
}
