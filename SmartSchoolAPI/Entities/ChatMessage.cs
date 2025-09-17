using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    /// <summary>
    /// يمثل رسالة واحدة ضمن محادثة في نظام المساعد الذكي.
    /// </summary>
    public class ChatMessage
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// المفتاح الخارجي للمحادثة التي تنتمي إليها هذه الرسالة.
        /// </summary>
        [Required]
        [Column("conversation_id")]
        public int ConversationId { get; set; }
 
        [Required]
        [MaxLength(50)]
        [Column("sender")]
        public string Sender { get; set; }

       
        [Required]
        [Column("content")]
        public string Content { get; set; }
 
        [Required]
        [Column("sent_at")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // --- Navigation Property ---
 
        [ForeignKey("ConversationId")]
        public virtual ChatConversation Conversation { get; set; }
    }
}