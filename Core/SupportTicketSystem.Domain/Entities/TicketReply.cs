namespace SupportTicketSystem.Domain.Entities;

public class TicketReply
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Message { get; set; }
    public Guid TicketId { get; set; }
    public required string SenderId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public virtual SupportTicket Ticket { get; set; } = null!;
    public virtual ApplicationUser Sender { get; set; } = null!;
}
