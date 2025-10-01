using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Course
{
    public class CreateCourseDto
    {
        [Required(ErrorMessage = "Course name is required.")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Academic Program ID is required.")]
        public int AcademicProgramId { get; set; }

         [Required(ErrorMessage = "Price is required.")]
        [Range(0, 99999999.99, ErrorMessage = "Price must be a non-negative value.")]
        public decimal Price { get; set; }
    }
}