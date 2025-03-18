using Microsoft.AspNetCore.Mvc;
using MediatR;
using SocialNetworkApi.Application.Common.DTOs;
using SocialNetworkApi.Application.Features.ChatMessages.Commands;

namespace SocialNetworkApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatMessageController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatMessageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public Task<ActionResult<IEnumerable<ChatMessageDto>>> GetChatMessages()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id}")]
        public Task<ActionResult<ChatMessageDto>> GetChatMessage(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<ActionResult<ChatMessageDto>> CreateChatMessage([FromBody] CreateChatMessageCommand request)
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
        public async Task<IActionResult> UpdateChatMessage(Guid id, [FromBody] UpdateChatMessageCommand request)
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
        public async Task<IActionResult> DeleteChatMessage(Guid id)
        {
            var request = new DeleteChatMessageCommand { Id = id };
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
