using SupportTicketSystem.Domain.Entities;

namespace SupportTicketSystem.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<SupportTicket> Tickets { get; }
    IGenericRepository<TicketReply> Replies { get; }
    IGenericRepository<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
