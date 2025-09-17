using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("enrollments")]
    public class Enrollment
    {
        [Key]
        [Column("enrollment_id")]
        public int EnrollmentId { get; set; }

        [Column("practical_grade", TypeName = "numeric(5, 2)")]
        public decimal? PracticalGrade { get; set; }

        [Column("exam_grade", TypeName = "numeric(5, 2)")]
        public decimal? ExamGrade { get; set; }

        [Column("final_grade", TypeName = "numeric(5, 2)")]
        public decimal? FinalGrade { get; set; }

        [Column("enrollment_date")]
        public DateTime EnrollmentDate { get; set; }

        // --- Foreign Keys ---
        [Required]
        [Column("student_id")]
        public int StudentId { get; set; }

        [Required]
        [Column("classroom_id")]
        public int ClassroomId { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("StudentId")]
        public User? Student { get; set; }

        [ForeignKey("ClassroomId")]
        public Classroom? Classroom { get; set; }
    }
}