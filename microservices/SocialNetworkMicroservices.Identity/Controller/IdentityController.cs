using System;
using Microsoft.AspNetCore.Mvc;

namespace SocialNetworkMicroservices.Identity.Controller;

public class IdentityController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult GetUserData()
    {
        // Simulate getting user data
        var user = new { Id = Guid.NewGuid(), Name = "John Doe" };
        return Ok(user);
    }
}
