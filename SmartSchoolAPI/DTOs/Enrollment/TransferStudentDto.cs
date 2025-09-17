using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Enrollment
{
    public class TransferStudentDto
    {
        [Required]
        public int NewClassroomId { get; set; }
    }
}
