using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Users.Commands;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, CommandResultDto<Guid>>
{
    private readonly IRepository<UserEntity> _userRepository;

    public DeleteUserCommandHandler(IRepository<UserEntity> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<CommandResultDto<Guid>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id);
        if (user == null)
        {
            return CommandResultDto<Guid>.Failure("User not found.");
        }

        await _userRepository.DeleteAsync(user);

        return CommandResultDto<Guid>.Success(user.Id);
    }
}
