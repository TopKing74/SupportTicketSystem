using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportTicketSystem.Domain.Entities;

namespace SupportTicketSystem.Persistence.Configurations;

public class TicketReplyConfiguration : IEntityTypeConfiguration<TicketReply>
{
    public void Configure(EntityTypeBuilder<TicketReply> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(r => r.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.SenderId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasOne(r => r.Ticket)
            .WithMany(t => t.Replies)
            .HasForeignKey(r => r.TicketId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(r => r.Sender)
            .WithMany(u => u.Replies)
            .HasForeignKey(r => r.SenderId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
