using Microsoft.EntityFrameworkCore;
using SupportTicketSystem.Application.Interfaces;
using SupportTicketSystem.Domain.Entities;
using SupportTicketSystem.Persistence.Context;
using SupportTicketSystem.Persistence.Repositories;

namespace SupportTicketSystem.Persistence;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;
    private GenericRepository<SupportTicket>? _tickets;
    private GenericRepository<TicketReply>? _replies;
    private GenericRepository<RefreshToken>? _refreshTokens;

    public IGenericRepository<SupportTicket> Tickets
        => _tickets ??= new GenericRepository<SupportTicket>(_context);

    public IGenericRepository<TicketReply> Replies
        => _replies ??= new GenericRepository<TicketReply>(_context);

    public IGenericRepository<RefreshToken> RefreshTokens
        => _refreshTokens ??= new GenericRepository<RefreshToken>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
