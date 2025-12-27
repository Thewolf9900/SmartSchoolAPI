// DTOs/Chat/ChatMessageDto.cs
using System;

namespace SmartSchoolAPI.DTOs.Chat
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
    }
}