using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.User
{
    public class AssignProgramDto
    {
        [Required(ErrorMessage = " .معرف البرنامج مطلوب")]
        public int AcademicProgramId { get; set; }
    }
}
