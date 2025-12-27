// DTOs/Chat/CreateMessageDto.cs
using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Chat
{
    public class CreateMessageDto
    {
        [Required]
        [MinLength(1)]
        public string Content { get; set; }
    }
}