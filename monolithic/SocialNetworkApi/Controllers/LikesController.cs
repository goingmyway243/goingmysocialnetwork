using Microsoft.AspNetCore.Mvc;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Features.Likes.Commands;
using Microsoft.AspNetCore.Authorization;

namespace SocialNetworkApi.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LikesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LikesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public Task<ActionResult<IEnumerable<LikeDto>>> GetLikes()
        {
            throw new NotImplementedException();
        }

        [HttpPost("{id}")]
        public Task<ActionResult<LikeDto>> GetLike(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<ActionResult<int>> ToggleLike([FromBody] ToggleLikeCommand request)
        {
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLike(Guid id, UpdateLikeCommand request)
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
    }
}
