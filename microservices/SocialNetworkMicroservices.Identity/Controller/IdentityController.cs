using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkMicroservices.Identity.Dtos;
using SocialNetworkMicroservices.Identity.Entities;

namespace SocialNetworkMicroservices.Identity.Controller;

public class IdentityController : ControllerBase
{
    private readonly UserManager<User> _userManager;

    public IdentityController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("me")]
    public IActionResult GetUserData()
    {
        // Simulate getting user data
        var user = new { Id = Guid.NewGuid(), Name = "John Doe" };
        return Ok(user);
    }

    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(user);
    }

    [HttpGet("user/email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(user);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        return Ok(users);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (model.Password != model.ConfirmPassword)
        {
            return BadRequest("Passwords do not match.");
        }

        var user = new User
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = $"{model.FirstName} {model.LastName}",
            DateOfBirth = model.DateOfBirth
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            return Ok(new { Message = "Registration successful. Please check your email to confirm." });
        }

        return BadRequest(result.Errors);
    }
}
