using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("graduations")]
    public class Graduation
    {
        [Key]
        [Column("graduation_id")]
        public int GraduationId { get; set; }

        [Column("student_user_id")]
        public int? StudentUserId { get; set; }

        [Required]
        [Column("first_name")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Column("last_name")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Column("national_id")]
        [StringLength(50)]
        public string? NationalId { get; set; }

        [Required]
        [Column("program_name_at_graduation")]
        [StringLength(255)]
        public string ProgramNameAtGraduation { get; set; } = string.Empty;

        [Required]
        [Column("graduation_date")]
        public DateTime GraduationDate { get; set; }

        [Column("final_gpa", TypeName = "numeric(5, 2)")]
        public decimal? FinalGpa { get; set; }

        // --- Foreign Key ---
        [Required]
        [Column("academic_program_id")]
        public int AcademicProgramId { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("AcademicProgramId")]
        public AcademicProgram? AcademicProgram { get; set; }

        public GraduationCertificate? Certificate { get; set; }
    }
}