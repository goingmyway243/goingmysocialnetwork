using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetworkApi.Domain.Entities;

namespace SocialNetworkApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatroomController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatroomEntity>>> GetChatrooms()
        {
            // Implementation for getting all chatrooms
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChatroomEntity>> GetChatroom(int id)
        {
            // Implementation for getting a chatroom by id
        }

        [HttpPost]
        public async Task<ActionResult<ChatroomEntity>> CreateChatroom(ChatroomEntity chatroom)
        {
            // Implementation for creating a new chatroom
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChatroom(int id, ChatroomEntity chatroom)
        {
            // Implementation for updating a chatroom
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChatroom(int id)
        {
            // Implementation for deleting a chatroom
        }
    }
}
