using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Course
{
    public class AssignCoordinatorDto
    {
        [Required(ErrorMessage = "معرف المدرس مطلوب.")]
        public int TeacherId { get; set; }
    }
}