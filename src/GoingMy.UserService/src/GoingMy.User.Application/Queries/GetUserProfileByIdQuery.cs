using GoingMy.User.Application.Commands;
using GoingMy.User.Application.Dtos;
using GoingMy.User.Domain.Repositories;
using MediatR;

namespace GoingMy.User.Application.Queries;

public record GetUserProfileByIdQuery(Guid Id) : IRequest<UserProfileDto?>;

public class GetUserProfileByIdQueryHandler(IUserProfileRepository userProfileRepository)
    : IRequestHandler<GetUserProfileByIdQuery, UserProfileDto?>
{
    public async Task<UserProfileDto?> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await userProfileRepository.GetByIdAsync(request.Id, cancellationToken);

        return profile is null ? null : CreateUserProfileCommandHandler.MapToDto(profile);
    }
}
