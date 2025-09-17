using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("password_reset_tokens")]
    public class PasswordResetToken
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("reset_code")]
        [StringLength(10)]
        public string ResetCode { get; set; } = string.Empty;

        [Required]
        [Column("expiry_date")]
        public DateTime ExpiryDate { get; set; }

        // --- Navigation Property ---
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}