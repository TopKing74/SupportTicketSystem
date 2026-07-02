using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SupportTicketSystem.Application.Interfaces;
using SupportTicketSystem.Domain.Entities;
using SupportTicketSystem.Domain.Enums;
using static SupportTicketSystem.Application.DTOs.ReplyDtos;
using static SupportTicketSystem.Application.DTOs.TicketDtos;

namespace SupportTicketSystem.Application.Services
{
    public class TicketService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper) : ITicketService
    {
        public async Task<TicketDetailsDto> CreateTicketAsync(CreateTicketDto dto)
        {
            var userId = currentUserService.UserId
                ?? throw new UnauthorizedAccessException("User is not authenticated.");

            var ticket = new SupportTicket
            {
                Title = dto.Title,
                Description = dto.Description,
                CustomerId = userId,
                Status = TicketStatus.Open
            };

            await unitOfWork.Tickets.AddAsync(ticket);
            await unitOfWork.SaveChangesAsync();

            var created = await unitOfWork.Tickets
                .GetAllQueryable()
                .Include(t => t.Customer)
                .Include(t => t.AssignedAgent)
                .AsNoTracking()
                .FirstAsync(t => t.Id == ticket.Id);

            return mapper.Map<TicketDetailsDto>(created);
        }

        public async Task<IEnumerable<TicketSummaryDto>> GetMyTicketsAsync()
        {
            var userId = currentUserService.UserId
                ?? throw new UnauthorizedAccessException("User is not authenticated.");

            var tickets = await unitOfWork.Tickets
                .GetAllQueryable()
                .Include(t => t.Customer)
                .Where(t => t.CustomerId == userId || t.AssignedAgentId == userId)
                .AsNoTracking()
                .ToListAsync();

            return mapper.Map<IEnumerable<TicketSummaryDto>>(tickets);
        }

        public async Task<IEnumerable<TicketSummaryDto>> GetAssignedTicketsAsync()
        {
            var userId = currentUserService.UserId
                ?? throw new UnauthorizedAccessException("User is not authenticated.");

            var tickets = await unitOfWork.Tickets
                .GetAllQueryable()
                .Include(t => t.Customer)
                .Where(t => t.AssignedAgentId == userId)
                .AsNoTracking()
                .ToListAsync();

            return mapper.Map<IEnumerable<TicketSummaryDto>>(tickets);
        }

        public async Task<TicketDetailsDto?> GetTicketByIdAsync(Guid id)
        {
            var ticket = await unitOfWork.Tickets
                .GetAllQueryable()
                .Include(t => t.Customer)
                .Include(t => t.AssignedAgent)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            return ticket is null ? null : mapper.Map<TicketDetailsDto>(ticket);
        }

        public async Task<IEnumerable<TicketSummaryDto>> SearchTicketsAsync(TicketSearchQuery query)
        {
            var dbQuery = unitOfWork.Tickets
                .GetAllQueryable()
                .Include(t => t.Customer)
                .Include(t => t.AssignedAgent)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Title))
            {
                var title = query.Title.Trim().ToLower();
                dbQuery = dbQuery.Where(t => t.Title.ToLower().Contains(title));
            }

            if (!string.IsNullOrWhiteSpace(query.Customer))
            {
                var customer = query.Customer.Trim().ToLower();
                dbQuery = dbQuery.Where(t =>
                    t.Customer.FullName.ToLower().Contains(customer) ||
                    t.CustomerId == query.Customer!.Trim());
            }

            if (query.Status is not null)
            {
                dbQuery = dbQuery.Where(t => t.Status == query.Status.Value);
            }

            if (query.FromDate is not null)
            {
                dbQuery = dbQuery.Where(t => t.CreatedAt >= query.FromDate.Value);
            }

            if (query.ToDate is not null)
            {
                dbQuery = dbQuery.Where(t => t.CreatedAt <= query.ToDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.AssignedAgent))
            {
                var agent = query.AssignedAgent.Trim().ToLower();
                dbQuery = dbQuery.Where(t =>
                    t.AssignedAgent != null &&
                    (t.AssignedAgent.FullName.ToLower().Contains(agent) ||
                     t.AssignedAgentId == query.AssignedAgent!.Trim()));
            }

            var tickets = await dbQuery
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return mapper.Map<IEnumerable<TicketSummaryDto>>(tickets);
        }

        public async Task<bool> UpdateTicketStatusAsync(Guid id, TicketStatus newStatus)
        {
            var ticket = await unitOfWork.Tickets.GetByIdAsync(id);
            if (ticket is null)
            {
                return false;
            }

            if (!IsValidTransition(ticket.Status, newStatus))
            {
                throw new InvalidOperationException(
                    $"Invalid status transition from '{ticket.Status}' to '{newStatus}'.");
            }

            ticket.Status = newStatus;
            ticket.LastUpdatedAt = DateTime.UtcNow;

            unitOfWork.Tickets.Update(ticket);
            await unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignAgentAsync(Guid id, string agentId)
        {
            var ticket = await unitOfWork.Tickets.GetByIdAsync(id);
            if (ticket is null)
            {
                return false;
            }

            ticket.AssignedAgentId = agentId;
            ticket.LastUpdatedAt = DateTime.UtcNow;

            if (ticket.Status == TicketStatus.Open)
            {
                ticket.Status = TicketStatus.InProgress;
            }

            unitOfWork.Tickets.Update(ticket);
            await unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ReplyDto> ReplyToTicketAsync(Guid id, CreateReplyDto dto)
        {
            var userId = currentUserService.UserId
                ?? throw new UnauthorizedAccessException("User is not authenticated.");

            var ticketExists = await unitOfWork.Tickets
                .GetAllQueryable()
                .AnyAsync(t => t.Id == id);
            if (!ticketExists)
            {
                throw new KeyNotFoundException($"Ticket with id '{id}' was not found.");
            }

            var reply = new TicketReply
            {
                Message = dto.Message,
                TicketId = id,
                SenderId = userId
            };

            await unitOfWork.Replies.AddAsync(reply);

            var ticket = await unitOfWork.Tickets.GetByIdAsync(id);
            if (ticket is not null)
            {
                ticket.LastUpdatedAt = DateTime.UtcNow;
                unitOfWork.Tickets.Update(ticket);
            }

            await unitOfWork.SaveChangesAsync();

            var created = await unitOfWork.Replies
                .GetAllQueryable()
                .Include(r => r.Sender)
                .AsNoTracking()
                .FirstAsync(r => r.Id == reply.Id);

            return mapper.Map<ReplyDto>(created);
        }

        public async Task<IEnumerable<ReplyDto>> GetTicketRepliesAsync(Guid id)
        {
            var replies = await unitOfWork.Replies
                .GetAllQueryable()
                .Include(r => r.Sender)
                .Where(r => r.TicketId == id)
                .OrderBy(r => r.Timestamp)
                .AsNoTracking()
                .ToListAsync();

            return mapper.Map<IEnumerable<ReplyDto>>(replies);
        }

        public async Task<DashboardCountsDto> GetDashboardCountsAsync()
        {
            var grouped = await unitOfWork.Tickets
                .GetAllQueryable()
                .AsNoTracking()
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var open = grouped.FirstOrDefault(x => x.Status == TicketStatus.Open)?.Count ?? 0;
            var inProgress = grouped.FirstOrDefault(x => x.Status == TicketStatus.InProgress)?.Count ?? 0;
            var resolved = grouped.FirstOrDefault(x => x.Status == TicketStatus.Resolved)?.Count ?? 0;
            var closed = grouped.FirstOrDefault(x => x.Status == TicketStatus.Closed)?.Count ?? 0;

            return new DashboardCountsDto(open, inProgress, resolved, closed, open + inProgress + resolved + closed);
        }

        private static bool IsValidTransition(TicketStatus current, TicketStatus target)
        {
            if (current == target)
            {
                return true;
            }

            return current switch
            {
                TicketStatus.Open => target == TicketStatus.InProgress || target == TicketStatus.Closed,
                TicketStatus.InProgress => target == TicketStatus.Resolved || target == TicketStatus.Closed,
                TicketStatus.Resolved => target == TicketStatus.Closed,
                TicketStatus.Closed => false,
                _ => false
            };
        }
    }
}
