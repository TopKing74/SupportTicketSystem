using SupportTicketSystem.Domain.Enums;

namespace SupportTicketSystem.Domain.Entities;

public class SupportTicket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Description { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public required string CustomerId { get; set; }
    public string? AssignedAgentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdatedAt { get; set; }

    public virtual ApplicationUser Customer { get; set; } = null!;
    public virtual ApplicationUser? AssignedAgent { get; set; }
    public virtual ICollection<TicketReply> Replies { get; set; } = [];
}
