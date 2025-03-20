using AutoMapper;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Common.Interfaces;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Enums;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IMapper _mapper;

    public IdentityService(
        IRepository<UserEntity> userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<AuthResult> GetUserById(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return AuthResult.Failure("User not found!");
        }

        return AuthResult.Success(_mapper.Map<UserDto>(user));
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

        var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
        if (existingUser != null)
        {
            return RegisterResult.Failure("User with this email already exists!");
        }

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            DateOfBirth = registerDto.DateOfBirth,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
        };

        try
        {
            await _userRepository.InsertAsync(user);
        }
        catch
        {
            return RegisterResult.Failure("Failed to create user!");
        }

        return RegisterResult.Success(user.Id, user.Email);
    }

    public async Task<AuthResult> PasswordSignInAsync(LoginDto loginDto)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return AuthResult.Failure("Invalid username or password!");
        }

        return AuthResult.Success(_mapper.Map<UserDto>(user));
    }

    public Task SignOutAsync()
    {
        return Task.CompletedTask;
    }

    public async Task<bool> IsUserInRoleAsync(Guid userId, UserRole role)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found!");
        }

        return user.HasRole(role);
    }

    public string GeneratePasswordHash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
