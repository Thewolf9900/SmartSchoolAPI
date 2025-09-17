using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Classroom
{
    public class UpdateClassroomDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }=string.Empty;

        [Required]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Capacity is required for update.")]
        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000.")]
        public int Capacity { get; set; }
    }
}
