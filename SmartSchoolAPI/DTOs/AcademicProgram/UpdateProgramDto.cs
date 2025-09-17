using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.AcademicProgram
{
    public class UpdateProgramDto
    {
        [Required(ErrorMessage = "اسم البرنامج مطلوب.")]
        [StringLength(255, ErrorMessage = "يجب ألا يتجاوز اسم البرنامج 255 حرفًا.")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}