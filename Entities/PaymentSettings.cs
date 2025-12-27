using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("payment_settings")]
    public class PaymentSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // لمنع الإنشاء التلقائي
        [Column("settings_id")]
        public int SettingsId { get; set; } = 1;

        [Required]
        [StringLength(200)]
        [Column("admin_full_name")]
        public string AdminFullName { get; set; }

        [Required]
        [StringLength(50)]
        [Column("phone_number")]
        public string PhoneNumber { get; set; }

        [Column("address")]
        public string? Address { get; set; }
    }
}