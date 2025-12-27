using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Enrollment
{
    public class SetRawGradesDto
    {
        [Required]
        [Range(0, 100, ErrorMessage = "Practical grade must be between 0 and 100.")]
        public decimal PracticalGrade { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Exam grade must be between 0 and 100.")]
        public decimal ExamGrade { get; set; }
    }
}
