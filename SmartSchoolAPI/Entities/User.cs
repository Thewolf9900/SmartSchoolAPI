using SmartSchoolAPI.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("first_name")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Column("last_name")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Column("email")]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Column("national_id")]
        [StringLength(50)]
        public string NationalId { get; set; } = string.Empty;

        [Required]
        [Column("role")]
        public UserRole Role { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // --- Foreign Key ---
        [Column("academic_program_id")]
        public int? AcademicProgramId { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("AcademicProgramId")]
        public AcademicProgram? AcademicProgram { get; set; }

        public ICollection<Classroom> TaughtClassrooms { get; set; } = new List<Classroom>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}