using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Quiz
{
    public class UpdateQuizQuestionOptionDto
    {
        public int? LectureQuizQuestionOptionId { get; set; }

        [Required(ErrorMessage = "نص الخيار مطلوب.")]
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}