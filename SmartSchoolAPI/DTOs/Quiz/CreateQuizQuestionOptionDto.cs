using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Quiz
{
    public class CreateQuizQuestionOptionDto
    {
        [Required(ErrorMessage = "نص الخيار مطلوب.")]
        public string Text { get; set; }=string.Empty;

        public bool IsCorrect { get; set; }
    }
}