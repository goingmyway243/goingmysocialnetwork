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

        var contents = request.Contents;
        if (contents.Count != 0)
        {
            contents.ForEach(async p =>
            {
                if (p.FormFile != null)
                {
                    var file = p.FormFile;
                    using (var stream = file.OpenReadStream())
                    {
                        p.LinkContent = await _storageService.UploadFileAsync(stream, file.Name, file.ContentType);
                    }
                }
            });
        }

        var post = _mapper.Map<PostEntity>(request);
        post.Id = Guid.NewGuid();
        post.Contents = contents.Select(_mapper.Map<ContentEntity>).ToList();

        await _postRepository.InsertAsync(post);

        post.User = existingUser;
        var result = _mapper.Map<PostDto>(post);

        return CommandResultDto<PostDto>.Success(result);
    }
}
