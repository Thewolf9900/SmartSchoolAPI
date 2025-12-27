// DTOs/Chat/ChatConversationDto.cs
using System;

namespace SmartSchoolAPI.DTOs.Chat
{
    public class ChatConversationDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}