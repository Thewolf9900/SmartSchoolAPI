using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("courses")]
    public class Course
    {
        [Key]
        [Column("course_id")]
        public int CourseId { get; set; }

        [Required]
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("price", TypeName = "numeric(10, 2)")]
        public decimal Price { get; set; }

        // --- Foreign Keys ---
        [Required]
        [Column("academic_program_id")]
        public int AcademicProgramId { get; set; }

        [Column("coordinator_id")]
        public int? CoordinatorId { get; set; } // المدرس المسؤول عن الدورة

        // --- Navigation Properties ---
        [ForeignKey("AcademicProgramId")]
        public AcademicProgram? AcademicProgram { get; set; }

        [ForeignKey("CoordinatorId")]
        public User? Coordinator { get; set; }

        public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
        public ICollection<Material> Materials { get; set; } = new List<Material>();
        public ICollection<Question> Questions { get; set; } = new List<Question>(); // لربط الدورة ببنك الأسئلة
        public ICollection<WeeklyChallengeSubmission> WeeklyChallengeSubmissions { get; set; } = new List<WeeklyChallengeSubmission>();
    }
}