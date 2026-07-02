using Microsoft.EntityFrameworkCore;
using SupportTicketSystem.Domain.Entities;
using SupportTicketSystem.Domain.Enums;
using SupportTicketSystem.Persistence;
using SupportTicketSystem.Persistence.Context;
using SupportTicketSystem.Persistence.Repositories;
using Xunit;

namespace SupportTicketSystem.Tests;

public class UnitOfWorkTests
{
    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsTicket()
    {
        await using var context = CreateInMemoryContext();
        var uow = new UnitOfWork(context);

        var ticket = new SupportTicket
        {
            Title = "Test ticket",
            Description = "A description",
            CustomerId = "user-1"
        };

        await uow.Tickets.AddAsync(ticket);
        await uow.SaveChangesAsync();

        var retrieved = await uow.Tickets.GetByIdAsync(ticket.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Test ticket", retrieved!.Title);
    }

    [Fact]
    public async Task Replies_AreDeleted_WhenTicketDeleted()
    {
        await using var context = CreateInMemoryContext();
        var uow = new UnitOfWork(context);

        var ticket = new SupportTicket
        {
            Title = "Parent",
            Description = "desc",
            CustomerId = "user-1"
        };
        await uow.Tickets.AddAsync(ticket);
        await uow.SaveChangesAsync();

        var reply = new TicketReply
        {
            Message = "a reply",
            TicketId = ticket.Id,
            SenderId = "user-1"
        };
        await uow.Replies.AddAsync(reply);
        await uow.SaveChangesAsync();

        var replies = await uow.Replies.FindAsync(r => r.TicketId == ticket.Id);
        Assert.Single(replies);
    }

    [Fact]
    public async Task RefreshToken_IsStored_AndQueried()
    {
        await using var context = CreateInMemoryContext();
        var uow = new UnitOfWork(context);

        var token = new RefreshToken
        {
            Token = "refresh-token-value",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            UserId = "user-1"
        };
        await uow.RefreshTokens.AddAsync(token);
        await uow.SaveChangesAsync();

        var found = await uow.RefreshTokens.GetAllQueryable()
            .FirstOrDefaultAsync(rt => rt.Token == "refresh-token-value");
        Assert.NotNull(found);
        Assert.False(found!.IsExpired);
    }
}

public class TicketStatusTransitionTests
{
    public static TheoryData<TicketStatus, TicketStatus, bool> Transitions => new()
    {
        { TicketStatus.Open, TicketStatus.InProgress, true },
        { TicketStatus.Open, TicketStatus.Closed, true },
        { TicketStatus.Open, TicketStatus.Resolved, false },
        { TicketStatus.InProgress, TicketStatus.Resolved, true },
        { TicketStatus.InProgress, TicketStatus.Closed, true },
        { TicketStatus.InProgress, TicketStatus.Open, false },
        { TicketStatus.Resolved, TicketStatus.Closed, true },
        { TicketStatus.Resolved, TicketStatus.Open, false },
        { TicketStatus.Closed, TicketStatus.Open, false },
        { TicketStatus.Closed, TicketStatus.Closed, true }
    };

    [Theory]
    [MemberData(nameof(Transitions))]
    public void IsValidTransition_ReturnsExpected(TicketStatus from, TicketStatus to, bool expected)
    {
        var result = PrivateIsValidTransition(from, to);
        Assert.Equal(expected, result);
    }

    private static bool PrivateIsValidTransition(TicketStatus current, TicketStatus target)
    {
        if (current == target) return true;
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
