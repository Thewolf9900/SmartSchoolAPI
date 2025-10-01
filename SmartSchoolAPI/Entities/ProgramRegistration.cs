using SmartSchoolAPI.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("program_registrations")]
    public class ProgramRegistration
    {
        [Key]
        [Column("registration_id")]
        public int RegistrationId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("first_name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        [Column("last_name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(255)]
        [Column("email")]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(50)]
        [Column("national_id")]
        public string NationalId { get; set; }

        [Required]
        [Column("academic_program_id")]
        public int AcademicProgramId { get; set; }

        [Required]
        [Column("status")]
        public RegistrationStatus Status { get; set; }

        [Required]
        [Column("request_date")]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [StringLength(512)]
        [Column("payment_receipt_url")]
        public string? PaymentReceiptUrl { get; set; }

        [StringLength(255)]
        [Column("payment_receipt_public_id")]
        public string? PaymentReceiptPublicId { get; set; }

        [Column("admin_notes")]
        public string? AdminNotes { get; set; }

        // Navigation Property
        [ForeignKey("AcademicProgramId")]
        public AcademicProgram AcademicProgram { get; set; }
    }
}