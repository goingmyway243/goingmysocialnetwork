using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikeController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeEntity>>> GetLikes()
        {
            // Implementation for getting all likes
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LikeEntity>> GetLike(int id)
        {
            // Implementation for getting a like by id
        }

        [HttpPost]
        public async Task<ActionResult<LikeEntity>> CreateLike(LikeEntity like)
        {
            // Implementation for creating a new like
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLike(int id, LikeEntity like)
        {
            // Implementation for updating a like
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLike(int id)
        {
            // Implementation for deleting a like
        }
    }
}
