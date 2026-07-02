using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketSystem.Application.Interfaces;
using static SupportTicketSystem.Application.DTOs.TicketDtos;

namespace SupportTicketSystem.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(ITicketService ticketService) : ControllerBase
{
    [HttpPut("tickets/{id:guid}/assign-agent")]
    public async Task<IActionResult> AssignAgent(Guid id, [FromBody] AssignAgentDto dto)
    {
        var assigned = await ticketService.AssignAgentAsync(id, dto.AgentId);
        return assigned ? NoContent() : NotFound();
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var counts = await ticketService.GetDashboardCountsAsync();
        return Ok(counts);
    }
}
