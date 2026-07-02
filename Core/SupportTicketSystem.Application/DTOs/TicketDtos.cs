using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupportTicketSystem.Domain.Enums;

namespace SupportTicketSystem.Application.DTOs
{
    public class TicketDtos
    {
        public record CreateTicketDto(string Title, string Description);
        public record TicketSummaryDto(Guid Id, string Title, TicketStatus Status, string CustomerName, DateTime CreatedAt);
        public record TicketDetailsDto(Guid Id, string Title, string Description, TicketStatus Status, string CustomerName, string? AssignedAgentName, DateTime CreatedAt, DateTime? LastUpdatedAt);
        public record UpdateStatusDto(TicketStatus Status);
        public record AssignAgentDto(string AgentId);

        public record TicketSearchQuery(
            string? Title,
            string? Customer,
            TicketStatus? Status,
            DateTime? FromDate,
            DateTime? ToDate,
            string? AssignedAgent);
    }
}
