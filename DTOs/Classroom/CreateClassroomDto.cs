using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Classroom
{
    public class CreateClassroomDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }=string.Empty;

        [Required]
        public int CourseId { get; set; }
        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100.")] 
        public int? Capacity { get; set; } 
    }
}
