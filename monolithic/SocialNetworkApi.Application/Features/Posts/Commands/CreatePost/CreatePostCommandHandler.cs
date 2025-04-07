using AutoMapper;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Common.Interfaces;
using SocialNetworkApi.Domain.Entities;
using SocialNetworkApi.Domain.Interfaces;

namespace SocialNetworkApi.Application.Features.Posts.Commands;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, CommandResultDto<PostDto>>
{
    private readonly IRepository<PostEntity> _postRepository;
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IStorageService _storageService;
    private readonly IMapper _mapper;

    public CreatePostCommandHandler(
        IRepository<PostEntity> postRepository,
        IRepository<UserEntity> userRepository,
        IStorageService storageService,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _storageService = storageService;
        _mapper = mapper;
    }

    public async Task<CommandResultDto<PostDto>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == default)
        {
            return CommandResultDto<PostDto>.Failure("Invalid user!");
        }

        var existingUser = await _userRepository.GetByIdAsync(request.UserId);
        if (existingUser == null)
        {
            return CommandResultDto<PostDto>.Failure("User not found!");
        }

        var post = _mapper.Map<PostEntity>(request);
        post.Id = Guid.NewGuid();

        var contents = request.Contents;
        for (int i = 0; i < contents.Count; i++)
        {
            var p = contents[i];
            p.Id = Guid.NewGuid();
            
            if (p.FormFile != null)
            {
                var file = p.FormFile;
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{post.Id}_content_{i}{extension}";
                using (var stream = file.OpenReadStream())
                {
                    p.LinkContent = await _storageService.UploadFileAsync(stream, fileName, file.ContentType, "posts");
                }
            }
        }

        post.Contents = contents.Select(_mapper.Map<ContentEntity>).ToList();

        await _postRepository.InsertAsync(post);

        var result = _mapper.Map<PostDto>(post);
        result.User = _mapper.Map<UserDto>(existingUser);

        return CommandResultDto<PostDto>.Success(result);
    }
}
