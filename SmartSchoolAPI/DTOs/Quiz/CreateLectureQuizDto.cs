using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Quiz
{
    public class CreateLectureQuizDto
    {
        [Required(ErrorMessage = "عنوان الاختبار مطلوب.")]
        [StringLength(255, ErrorMessage = "يجب ألا يتجاوز العنوان 255 حرفًا.")]
        public string Title { get; set; }
    }
}