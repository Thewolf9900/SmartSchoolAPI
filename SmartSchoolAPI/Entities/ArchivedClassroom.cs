using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("archived_classrooms")]
    public class ArchivedClassroom
    {
        [Key]
        [Column("archived_classroom_id")]
        public int ArchivedClassroomId { get; set; }

        [Required]
        [Column("original_classroom_id")]
        public int OriginalClassroomId { get; set; }

        [Required]
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("course_name")]
        [StringLength(255)]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        [Column("program_name")]
        [StringLength(255)]
        public string ProgramName { get; set; } = string.Empty;

        [Column("teacher_name")]
        [StringLength(200)]
        public string? TeacherName { get; set; }

        [Column("archived_at")]
        public DateTime ArchivedAt { get; set; }

        // --- Navigation Property ---
        public ICollection<ArchivedEnrollment> ArchivedEnrollments { get; set; } = new List<ArchivedEnrollment>();
    }
}