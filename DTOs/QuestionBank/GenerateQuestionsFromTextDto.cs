using SmartSchoolAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.QuestionBank
{
    public class GenerateQuestionsFromTextDto
    {
        [Required(ErrorMessage = "المحتوى النصي مطلوب.")]
        [MinLength(100, ErrorMessage = "يجب أن يكون النص 100 حرف على الأقل.")]
        public string ContextText { get; set; }

        [Range(1, 20, ErrorMessage = "يمكنك توليد ما بين 1 و 10 أسئلة في المرة الواحدة.")]
        public int NumberOfQuestions { get; set; } = 5;

        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;

        public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;

      
    }
}