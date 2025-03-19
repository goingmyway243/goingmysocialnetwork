using Microsoft.AspNetCore.Mvc;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Common.Interfaces;
using SocialNetworkApi.Domain.Enums;

namespace SocialNetworkApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public IdentityController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        var result = await _identityService.CreateUserAsync(request);
        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result.Error);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var result = await _identityService.PasswordSignInAsync(request);
        if (result.User != null)
        {
            return Ok(result.User);
        }

        return BadRequest(result.Error);
    }

    [HttpGet("check-role")]
    public async Task<bool> CheckUserInRole(Guid userId, string role)
    {
        if (!Enum.TryParse<UserRole>(role, true, out var roleEnum))
        {
            return false;
        }
        
        return await _identityService.IsUserInRoleAsync(userId, roleEnum);
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await _identityService.SignOutAsync();
        return Ok();
    }
}
