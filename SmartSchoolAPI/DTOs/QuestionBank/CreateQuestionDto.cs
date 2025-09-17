using Microsoft.AspNetCore.Http; // <-- إضافة
using SmartSchoolAPI.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.QuestionBank
{
    public class CreateQuestionDto
    {
        public string? Text { get; set; } 
        public IFormFile? Image { get; set; } 

        [Required(ErrorMessage = "نوع السؤال مطلوب.")]
        public QuestionType QuestionType { get; set; }

        [Required(ErrorMessage = "مستوى الصعوبة مطلوب.")]
        public DifficultyLevel DifficultyLevel { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "يجب توفير خيارين على الأقل.")]
        public List<CreateQuestionOptionDto> Options { get; set; } = new List<CreateQuestionOptionDto>();
    }
}