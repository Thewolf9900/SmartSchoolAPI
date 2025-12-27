using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.QuestionBank
{
    public class UpdateQuestionOptionDto
    {
         public int? QuestionOptionId { get; set; }

        [Required(ErrorMessage = "نص الخيار مطلوب.")]
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}