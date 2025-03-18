using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Users.Commands;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, CommandResult<Guid>>
{
    private readonly IRepository<UserEntity> _userRepository;

    public DeleteUserCommandHandler(IRepository<UserEntity> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<CommandResult<Guid>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return CommandResult<Guid>.Failure("User not found.");
        }

        await _userRepository.DeleteAsync(user);

        return CommandResult<Guid>.Success(user.Id);
    }
}
