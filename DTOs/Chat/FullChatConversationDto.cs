// DTOs/Chat/FullChatConversationDto.cs
using System.Collections.Generic;

namespace SmartSchoolAPI.DTOs.Chat
{
    public class FullChatConversationDto : ChatConversationDto
    {
        public List<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
    }
}