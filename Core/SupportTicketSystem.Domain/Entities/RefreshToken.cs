namespace SupportTicketSystem.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Token { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public required DateTime ExpiresAt { get; set; }
    public required string UserId { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public virtual ApplicationUser User { get; set; } = null!;
}
