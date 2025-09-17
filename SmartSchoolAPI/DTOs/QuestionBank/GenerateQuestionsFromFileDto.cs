using SmartSchoolAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.QuestionBank
{
    public class GenerateQuestionsFromFileDto
    {
        [Required(ErrorMessage = "الملف مطلوب.")]
        public IFormFile File { get; set; }

        [Range(1, 20, ErrorMessage = "يمكنك توليد ما بين 1 و 10 أسئلة في المرة الواحدة.")]
        public int NumberOfQuestions { get; set; } = 5;

        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;

        public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;

    }
}