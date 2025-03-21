using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Common.Interfaces;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Users.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CommandResultDto<UserDto>>
{
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(
        IRepository<UserEntity> userRepository, 
        IIdentityService identityService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<CommandResultDto<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return CommandResultDto<UserDto>.Failure("Email and password are required!");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return CommandResultDto<UserDto>.Failure("Full name is required!");
        }

        if (request.DateOfBirth == default || request.DateOfBirth < DateTime.Now.AddYears(-100) || request.DateOfBirth > DateTime.Now)
        {
            return CommandResultDto<UserDto>.Failure("Your date of birth is invalid!");
        }

        var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return CommandResultDto<UserDto>.Failure("User with this email already exists!");
        }

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Role = request.Role,
            DateOfBirth = request.DateOfBirth,
            FullName = request.FullName,
            ProfilePicture = request.ProfilePicture,
            Bio = request.Bio,
            Location = request.Location,
            Website = request.Website,
            Gender = request.Gender,
            Address = request.Address,
            City = request.City
        };

        user.PasswordHash = _identityService.GeneratePasswordHash(request.Password);

        await _userRepository.InsertAsync(user);

        var result = _mapper.Map<UserDto>(user);
        return CommandResultDto<UserDto>.Success(result);
    }
}
