using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    /// <summary>
    /// يمثل محادثة واحدة في نظام المساعد الذكي.
    /// كل محادثة ترتبط بمستخدم معين وتحتوي على مجموعة من الرسائل.
    /// </summary>
    public class ChatConversation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        
        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

      
        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; }
 
        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties ---

       
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

       
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}