using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Common.Interfaces;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Users.Commands;

public class UpdateAvatarCommandHandler : IRequestHandler<UpdateAvatarCommand, CommandResultDto<string>>
{
    private readonly IStorageService _storageService;
    private readonly IRepository<UserEntity> _userRepository;

    public UpdateAvatarCommandHandler(IRepository<UserEntity> userRepository, IStorageService storageService)
    {
        _userRepository = userRepository;
        _storageService = storageService;
    }

    public async Task<CommandResultDto<string>> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return CommandResultDto<string>.Failure("User not found.");
        }

        var file = request.FormFile;
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{user.Id}_avatar{extension}";
        using (var stream = file.OpenReadStream())
        {
            user.ProfilePicture = await _storageService.UploadFileAsync(stream, fileName, file.ContentType, "avatars");
        }

        await _userRepository.UpdateAsync(user);

        return CommandResultDto<string>.Success(user.ProfilePicture);
    }
}
