using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentEntity>>> GetComments()
        {
            // Implementation for getting all comments
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommentEntity>> GetComment(int id)
        {
            // Implementation for getting a comment by id
        }

        [HttpPost]
        public async Task<ActionResult<CommentEntity>> CreateComment(CommentEntity comment)
        {
            // Implementation for creating a new comment
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, CommentEntity comment)
        {
            // Implementation for updating a comment
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            // Implementation for deleting a comment
        }
    }
}