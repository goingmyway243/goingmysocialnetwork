using Microsoft.AspNetCore.Mvc;
using SocialNetworkApi.Domain.Entities;
using MediatR;

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
        
        [HttpGet]
        public Task<ActionResult<IEnumerable<FriendshipEntity>>> GetFriendships()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id}")]
        public Task<ActionResult<FriendshipEntity>> GetFriendship(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public Task<ActionResult<FriendshipEntity>> CreateFriendship(FriendshipEntity friendship)
        {
            throw new NotImplementedException();
        }

        [HttpPut("{id}")]
        public Task<IActionResult> UpdateFriendship(int id, FriendshipEntity friendship)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{id}")]
        public Task<IActionResult> DeleteFriendship(int id)
        {
            throw new NotImplementedException();
        }
    }
}
