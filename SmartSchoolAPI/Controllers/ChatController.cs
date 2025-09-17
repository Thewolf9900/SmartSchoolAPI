using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Chat;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepo;
        private readonly IAiService _aiService; 

        public ChatController(IChatRepository chatRepo, IAiService aiService)
        {
            _chatRepo = chatRepo;
            _aiService = aiService;
        }

        [HttpGet("conversations")]
        public async Task<ActionResult<IEnumerable<ChatConversationDto>>> GetMyConversations()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var conversations = await _chatRepo.GetConversationsByUserIdAsync(userId.Value);
            var dtos = conversations.Select(c => new ChatConversationDto
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt
            });
            return Ok(dtos);
        }

        [HttpGet("conversations/{id}")]
        public async Task<ActionResult<FullChatConversationDto>> GetConversation(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var conversation = await _chatRepo.GetConversationWithMessagesAsync(id, userId.Value);
            if (conversation == null)
            {
                return NotFound(new { message = "المحادثة غير موجودة أو لا تملك صلاحية الوصول إليها." });
            }
            var dto = new FullChatConversationDto
            {
                Id = conversation.Id,
                Name = conversation.Name,
                CreatedAt = conversation.CreatedAt,
                Messages = conversation.Messages.Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    Sender = m.Sender,
                    Content = m.Content,
                    SentAt = m.SentAt
                }).ToList()
            };
            return Ok(dto);
        }

        [HttpPost("conversations")]
        public async Task<ActionResult<ChatConversationDto>> CreateConversation()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var newConversation = new ChatConversation
            {
                UserId = userId.Value,
                Name = $"محادثة جديدة",
                CreatedAt = DateTime.UtcNow
            };
            await _chatRepo.AddConversationAsync(newConversation);
            await _chatRepo.SaveChangesAsync();
            var dto = new ChatConversationDto
            {
                Id = newConversation.Id,
                Name = newConversation.Name,
                CreatedAt = newConversation.CreatedAt
            };
            return CreatedAtAction(nameof(GetConversation), new { id = dto.Id }, dto);
        }

        [HttpPost("conversations/{id}/messages")]
        public async Task<ActionResult<ChatMessageDto>> PostMessage(int id, [FromBody] CreateMessageDto messageDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var conversation = await _chatRepo.GetConversationWithMessagesAsync(id, userId.Value);
            if (conversation == null)
            {
                return NotFound(new { message = "المحادثة غير موجودة." });
            }

             var userMessage = new ChatMessage
            {
                ConversationId = id,
                Sender = "User",
                Content = messageDto.Content,
                SentAt = DateTime.UtcNow
            };
            await _chatRepo.AddMessageAsync(userMessage);
            await _chatRepo.SaveChangesAsync();

             conversation.Messages.Add(userMessage);

             var assistantResponseContent = await _aiService.GetChatResponseAsync(conversation.Messages);

             var assistantMessage = new ChatMessage
            {
                ConversationId = id,
                Sender = "Assistant",
                Content = assistantResponseContent,
                SentAt = DateTime.UtcNow
            };
            await _chatRepo.AddMessageAsync(assistantMessage);
            await _chatRepo.SaveChangesAsync();

            var assistantMessageDto = new ChatMessageDto
            {
                Id = assistantMessage.Id,
                Sender = assistantMessage.Sender,
                Content = assistantMessage.Content,
                SentAt = assistantMessage.SentAt
            };

            return Ok(assistantMessageDto);
        }

        [HttpDelete("conversations/{id}")]
        public async Task<IActionResult> DeleteConversation(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var conversation = await _chatRepo.GetConversationWithMessagesAsync(id, userId.Value);
            if (conversation == null)
            {
                return NotFound(new { message = "المحادثة غير موجودة." });
            }
            _chatRepo.DeleteConversation(conversation);
            await _chatRepo.SaveChangesAsync();
            return NoContent();
        }

        #region Helper Methods
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
        #endregion
    }
}