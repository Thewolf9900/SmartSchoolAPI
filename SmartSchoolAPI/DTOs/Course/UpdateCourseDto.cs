using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Course
{
    public class UpdateCourseDto
    {
        [Required(ErrorMessage = "Course name is required.")]
        [MaxLength(255)]
        public string Name { get; set; }=string.Empty;
    }
}
