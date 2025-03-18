using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Users.Commands;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, CommandResult<UserDto>>
{
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(IRepository<UserEntity> userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<CommandResult<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id);
        if (user == null)
        {
            return CommandResult<UserDto>.Failure("User not found.");
        }

        user.FullName = request.FullName ?? user.FullName;
        user.Bio = request.Bio ?? user.Bio;
        user.ProfilePicture = request.ProfilePicture ?? user.ProfilePicture;
        user.Location = request.Location ?? user.Location;
        user.Website = request.Website ?? user.Website;
        user.Address = request.Address ?? user.Address;
        user.City = request.City ?? user.City;
        user.Gender = request.Gender ?? user.Gender;
        user.DateOfBirth = request.DateOfBirth ?? user.DateOfBirth;

        await _userRepository.UpdateAsync(user);

        var result = _mapper.Map<UserDto>(user);
        return CommandResult<UserDto>.Success(result);
    }
}
