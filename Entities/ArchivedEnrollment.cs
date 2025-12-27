using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("archived_enrollments")]
    public class ArchivedEnrollment
    {
        [Key]
        [Column("archived_enrollment_id")]
        public int ArchivedEnrollmentId { get; set; }

        [Required]
        [Column("archived_classroom_id")]
        public int ArchivedClassroomId { get; set; }

        [Required]
        [Column("student_name")]
        [StringLength(200)]
        public string StudentName { get; set; } = string.Empty;

        [Column("student_national_id")]
        [StringLength(50)]
        public string? StudentNationalId { get; set; }

        [Column("practical_grade", TypeName = "numeric(5, 2)")]
        public decimal? PracticalGrade { get; set; }

        [Column("exam_grade", TypeName = "numeric(5, 2)")]
        public decimal? ExamGrade { get; set; }

        [Column("final_grade", TypeName = "numeric(5, 2)")]
        public decimal? FinalGrade { get; set; }

        // --- Navigation Property ---
        [ForeignKey("ArchivedClassroomId")]
        public ArchivedClassroom? ArchivedClassroom { get; set; }
    }
}