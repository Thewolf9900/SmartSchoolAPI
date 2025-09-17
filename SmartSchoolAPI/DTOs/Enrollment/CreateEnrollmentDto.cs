using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Enrollment
{
    public class CreateEnrollmentDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int ClassroomId { get; set; }
    }
}
