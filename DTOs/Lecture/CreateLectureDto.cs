using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Lecture
{
    public class CreateLectureDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }=string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, 100)]  
        public int LectureOrder { get; set; }
    }
}
