using Microsoft.AspNetCore.Mvc;
using MediatR;
using SocialNetworkApi.Application.Features.Users.Queries;
using SocialNetworkApi.Application.Features.Users.Commands;
using Microsoft.AspNetCore.Authorization;

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