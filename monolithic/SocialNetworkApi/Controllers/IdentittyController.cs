using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
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
    private readonly IJwtService _jwtService;

    public IdentityController(IIdentityService identityService, IJwtService jwtService)
    {
        _identityService = identityService;
        _jwtService = jwtService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetUserData()
    {
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(nameIdentifier) || !Guid.TryParse(nameIdentifier, out Guid userId))
        {
            return Unauthorized("You don't have permission to access!");
        }

        var result = await _identityService.GetUserById(userId);
        if (result.User == null)
        {
            return BadRequest(result.Error);
        }

        var token = _jwtService.GenerateAccessToken(result.User);
        return Ok(new { result.User, Token = token });
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
        if (result.User == null)
        {
            return BadRequest(result.Error);
        }

        var token = _jwtService.GenerateAccessToken(result.User);
        return Ok(new { result.User, Token = token });
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
