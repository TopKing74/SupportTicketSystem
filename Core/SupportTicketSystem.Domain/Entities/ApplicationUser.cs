using Microsoft.AspNetCore.Identity;
using SupportTicketSystem.Domain.Enums;

namespace SupportTicketSystem.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public required string FullName { get; set; }
    public required string ContactNumber { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public virtual ICollection<SupportTicket> CreatedTickets { get; set; } = [];
    public virtual ICollection<SupportTicket> AssignedTickets { get; set; } = [];
    public virtual ICollection<TicketReply> Replies { get; set; } = [];
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
