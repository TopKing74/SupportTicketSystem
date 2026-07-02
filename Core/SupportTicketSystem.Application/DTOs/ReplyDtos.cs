using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTicketSystem.Application.DTOs
{
    public class ReplyDtos
    {
        public record CreateReplyDto(string Message);
        public record ReplyDto(Guid Id, string Message, string SenderName, DateTime Timestamp);
    }
}
