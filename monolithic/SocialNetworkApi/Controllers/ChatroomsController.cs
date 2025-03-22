using Microsoft.AspNetCore.Mvc;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Features.Chatrooms.Commands;
using MediatR;
using SocialNetworkApi.Application.Features.Chatrooms.Queries;

namespace SocialNetworkApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatroomsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatroomsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("search")]
        public async Task<ActionResult<PagedResultDto<ChatroomDto>>> SearchChatrooms([FromBody] SearchChatroomsQuery request)
        {
            var result = await _mediator.Send(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public Task<ActionResult<ChatroomDto>> GetChatroom(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<ActionResult<ChatroomDto>> CreateChatroom([FromBody] CreateChatroomCommand request)
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
        public async Task<IActionResult> UpdateChatroom(Guid id, [FromBody] UpdateChatroomCommand request)
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
        public async Task<IActionResult> DeleteChatroom(Guid id)
        {
            var request = new DeleteChatroomCommand { Id = id };
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
