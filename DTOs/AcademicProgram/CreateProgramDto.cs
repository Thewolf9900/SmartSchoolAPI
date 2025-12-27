using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.AcademicProgram
{
    /// <summary>
    /// نموذج لنقل البيانات المستخدمة في إنشاء برنامج أكاديمي جديد.
    /// </summary>
    public class CreateProgramDto
    {
        /// 
         [StringLength(255, ErrorMessage = "يجب ألا يتجاوز اسم البرنامج 255 حرفًا.")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}