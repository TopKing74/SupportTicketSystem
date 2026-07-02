using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportTicketSystem.Application.Interfaces;
using SupportTicketSystem.Domain.Entities;
using static SupportTicketSystem.Application.DTOs.AuthDtos;

namespace SupportTicketSystem.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenGenerator jwtTokenGenerator,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await userManager.FindByEmailAsync(dto.Email) is not null)
        {
            return Conflict(new { message = "A user with this email already exists." });
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            ContactNumber = dto.ContactNumber
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        await userManager.AddToRoleAsync(user, "Customer");

        var roles = await userManager.GetRolesAsync(user);
        var authResponse = jwtTokenGenerator.GenerateTokens(user, roles);
        await PersistRefreshTokenAsync(user.Id, authResponse.RefreshToken);

        return Ok(authResponse);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var roles = await userManager.GetRolesAsync(user);
        var authResponse = jwtTokenGenerator.GenerateTokens(user, roles);
        await PersistRefreshTokenAsync(user.Id, authResponse.RefreshToken);

        return Ok(authResponse);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshRequestDto request)
    {
        var storedToken = await unitOfWork.RefreshTokens
            .GetAllQueryable()
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken is null || storedToken.IsExpired)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }

        var user = await userManager.FindByIdAsync(storedToken.UserId);
        if (user is null)
        {
            return Unauthorized(new { message = "User not found." });
        }

        unitOfWork.RefreshTokens.Delete(storedToken);

        var roles = await userManager.GetRolesAsync(user);
        var authResponse = jwtTokenGenerator.GenerateTokens(user, roles);
        await PersistRefreshTokenAsync(user.Id, authResponse.RefreshToken);

        return Ok(authResponse);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var userTokens = await unitOfWork.RefreshTokens
            .FindAsync(rt => rt.UserId == userId);
        foreach (var token in userTokens)
        {
            unitOfWork.RefreshTokens.Delete(token);
        }

        await unitOfWork.SaveChangesAsync();
        await signInManager.SignOutAsync();
        return NoContent();
    }

    private async Task PersistRefreshTokenAsync(string userId, string token)
    {
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await unitOfWork.SaveChangesAsync();
    }

    public record RefreshRequestDto(string RefreshToken);
}
