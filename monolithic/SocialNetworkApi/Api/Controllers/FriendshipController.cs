using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendshipController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FriendshipEntity>>> GetFriendships()
        {
            // Implementation for getting all friendships
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FriendshipEntity>> GetFriendship(int id)
        {
            // Implementation for getting a friendship by id
        }

        [HttpPost]
        public async Task<ActionResult<FriendshipEntity>> CreateFriendship(FriendshipEntity friendship)
        {
            // Implementation for creating a new friendship
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFriendship(int id, FriendshipEntity friendship)
        {
            // Implementation for updating a friendship
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFriendship(int id)
        {
            // Implementation for deleting a friendship
        }
    }
}
