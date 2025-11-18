using System;

namespace SocialNetworkMicroservices.Identity;

public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
