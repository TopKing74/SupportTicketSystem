using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SupportTicketSystem.Domain.Entities;
using static SupportTicketSystem.Application.DTOs.ReplyDtos;
using static SupportTicketSystem.Application.DTOs.TicketDtos;

namespace SupportTicketSystem.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SupportTicket, TicketSummaryDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName));

            CreateMap<SupportTicket, TicketDetailsDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName))
                .ForMember(dest => dest.AssignedAgentName, opt => opt.MapFrom(src => src.AssignedAgent != null ? src.AssignedAgent.FullName : null));

            CreateMap<TicketReply, ReplyDto>()
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender.FullName));
        }
    }
}
