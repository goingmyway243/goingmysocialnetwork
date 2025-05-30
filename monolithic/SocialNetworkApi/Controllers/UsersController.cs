using Microsoft.AspNetCore.Mvc;
using MediatR;
using SocialNetworkApi.Application.Features.Users.Queries;
using SocialNetworkApi.Application.Features.Users.Commands;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SocialNetworkApi.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetUserByIdQuery { Id = id });
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchUsers([FromBody] SearchUsersQuery request)
        {
            if (request.IncludeFriendship && !request.RequestUserId.HasValue)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(currentUserId, out Guid userId))
                {
                    request.RequestUserId = userId;
                }
            }

            var result = await _mediator.Send(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserCommand request)
        {
            var result = await _mediator.Send(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand request)
        {
            if (id != request.Id)
            {
                return BadRequest("Your request is invalid!");
            }

            var result = await _mediator.Send(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        [HttpPut("{id}/avatar")]
        public async Task<IActionResult> UpdateAvatar(Guid id, IFormFile file)
        {
            if (id == Guid.Empty || file == null)
            {
                return BadRequest("Your request is invalid!");
            }

            var result = await _mediator.Send(new UpdateAvatarCommand { UserId = id, FormFile = file });
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(new { Url = result.Data});
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteUserCommand { Id = id });
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }
    }
}