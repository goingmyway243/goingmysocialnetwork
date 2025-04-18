using Microsoft.AspNetCore.Mvc;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Features.Comments.Commands;
using MediatR;
using SocialNetworkApi.Application.Features.Comments.Queries;
using Microsoft.AspNetCore.Authorization;

namespace SocialNetworkApi.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CommentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("search")]
        public async Task<ActionResult<PagedResultDto<CommentDto>>> SearchComments([FromBody] SearchCommentsQuery request)
        {
            var result = await _mediator.Send(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public Task<ActionResult<CommentDto>> GetComment(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CreateCommentCommand request)
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
        public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentCommand request)
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
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            var request = new DeleteCommentCommand { Id = id };
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