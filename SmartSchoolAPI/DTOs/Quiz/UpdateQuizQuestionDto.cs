using SmartSchoolAPI.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Quiz
{
    public class UpdateQuizQuestionDto
    {
        [Required(ErrorMessage = "نص السؤال مطلوب.")]
        public string Text { get; set; }

        [Required(ErrorMessage = "نوع السؤال مطلوب.")]
        public QuestionType QuestionType { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "يجب توفير خيارين على الأقل.")]
        public List<UpdateQuizQuestionOptionDto> Options { get; set; } = new List<UpdateQuizQuestionOptionDto>();
    }
}