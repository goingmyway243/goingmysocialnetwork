using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContentController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContentEntity>>> GetContents()
        {
            // Implementation for getting all contents
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContentEntity>> GetContent(int id)
        {
            // Implementation for getting a content by id
        }

        [HttpPost]
        public async Task<ActionResult<ContentEntity>> CreateContent(ContentEntity content)
        {
            // Implementation for creating a new content
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContent(int id, ContentEntity content)
        {
            // Implementation for updating a content
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContent(int id)
        {
            // Implementation for deleting a content
        }
    }
}
