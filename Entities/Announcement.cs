using SmartSchoolAPI.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("announcements")]
    public class Announcement
    {
        [Key]
        [Column("announcement_id")]
        public int AnnouncementId { get; set; }

        [Required]
        [Column("title")]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("posted_at")]
        public DateTime PostedAt { get; set; }

        [Required]
        [Column("target_scope")]
        public AnnouncementScope TargetScope { get; set; }

        // --- Foreign Keys ---
        [Column("academic_program_id")]
        public int? AcademicProgramId { get; set; }

        [Column("course_id")]
        public int? CourseId { get; set; }

        [Column("classroom_id")]
        public int? ClassroomId { get; set; }

        [Column("created_by_user_id")]
        public int? CreatedByUserId { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("AcademicProgramId")]
        public AcademicProgram? AcademicProgram { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        [ForeignKey("ClassroomId")]
        public Classroom? Classroom { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? Creator { get; set; }
    }
}