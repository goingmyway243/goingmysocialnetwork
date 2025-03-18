using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Social.UserService.Domain.Entities;

namespace Social.UserService.Infrastructure.Identity
{
  public class IdentityService
  {
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;

    public IdentityService(UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager)
    {
      _userManager = userManager;
      _signInManager = signInManager;
    }

    public async Task<AuthResult> CreateUserAsync(string email, string password, string fullName, DateTime dateOfBirth)
    {
      var user = new UserEntity { UserName = email, Email = email, FullName = fullName, DateOfBirth = dateOfBirth };
      var result = await _userManager.CreateAsync(user, password);

      if (!result.Succeeded)
      {
        throw new Exception("Failed to create user!");
      }

      return new AuthResult(user.Id, user.Email ?? string.Empty, GenerateJwtToken(user));
    }

    public async Task<AuthResult> PasswordSignInAsync(string email, string password)
    {
      var user = await _userManager.FindByEmailAsync(email);
      if (user == null || !await _userManager.CheckPasswordAsync(user, password))
      {
        throw new Exception("Invalid username or password!");
      }

      return new AuthResult(user.Id, user.Email ?? string.Empty, GenerateJwtToken(user));
    }

    public async Task SignOutAsync()
    {
      await _signInManager.SignOutAsync();
    }

    public string GenerateJwtToken(UserEntity user)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes("GoingMyJWTSecretKey"); // Use a secure key
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new Claim[]
          {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // Add additional claims as needed
          }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        Issuer = "https://localhost:7237",
        Audience = "http://localhost:4200"
      };
      var token = tokenHandler.CreateToken(tokenDescriptor);
      return tokenHandler.WriteToken(token);
    }
  }
}
