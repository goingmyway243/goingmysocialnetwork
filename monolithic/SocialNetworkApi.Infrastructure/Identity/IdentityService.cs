using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Common.Interfaces;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Enums;

namespace SocialNetworkApi.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly HttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public IdentityService(
        UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager,
        HttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<RegisterResult> CreateUserAsync(RegisterDto registerDto)
    {
        if (string.IsNullOrWhiteSpace(registerDto.Email) || string.IsNullOrWhiteSpace(registerDto.Password))
        {
            return RegisterResult.Failure("Email and password are required!");
        }

        if (string.IsNullOrWhiteSpace(registerDto.FullName))
        {
            return RegisterResult.Failure("Full name is required!");
        }

        if (registerDto.DateOfBirth == default || registerDto.DateOfBirth < DateTime.Now.AddYears(-100) || registerDto.DateOfBirth > DateTime.Now)
        {
            return RegisterResult.Failure("Your date of birth is invalid!");
        }

        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return RegisterResult.Failure("User with this email already exists!");
        }

        var user = new UserEntity
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            DateOfBirth = registerDto.DateOfBirth
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return RegisterResult.Failure("Failed to create user!");
        }

        return RegisterResult.Success(user.Id, user.UserName);
    }

    public async Task<AuthResult> PasswordSignInAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return AuthResult.Failure("Invalid username or password!");
        }

        return AuthResult.Success(_mapper.Map<UserDto>(user));
    }

    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<bool> IsUserInRoleAsync(string userId, UserRole role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found!");
        }

        return user.HasRole(role);
    }

    public Guid GetCurrentUserId()
    {
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userId, out var result))
        {
            return result;
        }

        return default;
    }

    public string GeneratePasswordHash(UserEntity user, string password)
    {
        return _userManager.PasswordHasher.HashPassword(user, password);
    }
}
