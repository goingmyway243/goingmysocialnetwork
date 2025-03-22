using Microsoft.AspNetCore.Mvc;
using SocialNetworkApi.Domain.Entities;
using MediatR;
using SocialNetworkApi.Application.Features.Friendships.Commands;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Features.Friendships.Queries;

namespace SocialNetworkApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendshipsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FriendshipsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("search")]
        public async Task<ActionResult<PagedResultDto<FriendshipDto>>> SearchFriendships([FromBody] SearchFriendshipsQuery request)
        {
            var result = await _mediator.Send(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public Task<ActionResult<FriendshipEntity>> GetFriendship(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<ActionResult<FriendshipEntity>> CreateFriendship([FromBody] CreateFriendshipCommand request)
        {
            var result = await _mediator.Send(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFriendship(Guid id, [FromBody] UpdateFriendshipCommand request)
        {
            if (id != request.Id)
            {
                return BadRequest("Your request is invalid!");
            }

            var result = await _mediator.Send(request);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result.Error);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFriendship(Guid id)
        {
            var request = new DeleteFriendshipCommand { Id = id };
            var result = await _mediator.Send(request);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result.Error);
            }
        }
    }
}
