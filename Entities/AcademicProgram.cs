using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("academic_programs")]
    public class AcademicProgram
    {
        [Key]
        [Column("academic_program_id")]
        public int AcademicProgramId { get; set; }

        [Required]
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("is_registration_open")]
        public bool IsRegistrationOpen { get; set; }

        // --- Navigation Properties ---
        public ICollection<Course> Courses { get; set; } = new List<Course>();
        public ICollection<User> Users { get; set; } = new List<User>();
         public ICollection<ProgramRegistration> ProgramRegistrations { get; set; } = new List<ProgramRegistration>();
    }
}