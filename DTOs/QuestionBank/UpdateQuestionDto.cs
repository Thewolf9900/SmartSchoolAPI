using SmartSchoolAPI.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.QuestionBank
{
    public class UpdateQuestionDto
    {
        [Required(ErrorMessage = "نص السؤال مطلوب.")]
        public string? Text { get; set; }

        public IFormFile? NewImage { get; set; }
        public bool DeleteCurrentImage { get; set; } = false;


        [Required(ErrorMessage = "نوع السؤال مطلوب.")]
        public QuestionType QuestionType { get; set; }

        [Required(ErrorMessage = "مستوى الصعوبة مطلوب.")]
        public DifficultyLevel DifficultyLevel { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "يجب توفير خيارين على الأقل.")]
        public List<UpdateQuestionOptionDto> Options { get; set; } = new List<UpdateQuestionOptionDto>();
    }
}