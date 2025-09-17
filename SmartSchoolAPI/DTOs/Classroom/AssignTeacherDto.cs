using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Classroom
{
    public class AssignTeacherDto
    {
        [Required]
        public int TeacherId { get; set; }
    }
}
