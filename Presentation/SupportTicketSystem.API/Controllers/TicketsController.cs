using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketSystem.Application.Interfaces;
using SupportTicketSystem.Domain.Enums;
using static SupportTicketSystem.Application.DTOs.ReplyDtos;
using static SupportTicketSystem.Application.DTOs.TicketDtos;

namespace SupportTicketSystem.API.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController(ITicketService ticketService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
    {
        var created = await ticketService.CreateTicketAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetMyTickets()
    {
        var tickets = await ticketService.GetMyTicketsAsync();
        return Ok(tickets);
    }

    [HttpGet("assigned")]
    public async Task<IActionResult> GetAssignedTickets()
    {
        var tickets = await ticketService.GetAssignedTicketsAsync();
        return Ok(tickets);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] TicketSearchQuery query)
    {
        var tickets = await ticketService.SearchTicketsAsync(query);
        return Ok(tickets);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ticket = await ticketService.GetTicketByIdAsync(id);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
    {
        try
        {
            var updated = await ticketService.UpdateTicketStatusAsync(id, dto.Status);
            return updated ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reply")]
    public async Task<IActionResult> Reply(Guid id, [FromBody] CreateReplyDto dto)
    {
        try
        {
            var reply = await ticketService.ReplyToTicketAsync(id, dto);
            return CreatedAtAction(nameof(GetReplies), new { id }, reply);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/replies")]
    public async Task<IActionResult> GetReplies(Guid id)
    {
        var replies = await ticketService.GetTicketRepliesAsync(id);
        return Ok(replies);
    }
}
