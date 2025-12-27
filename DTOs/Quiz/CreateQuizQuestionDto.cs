    using Microsoft.AspNetCore.Http;
    using SmartSchoolAPI.Enums;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    namespace SmartSchoolAPI.DTOs.Quiz
    {
        public class CreateQuizQuestionDto
        {
            public string? Text { get; set; } 
            public IFormFile? Image { get; set; } 

            [Required(ErrorMessage = "نوع السؤال مطلوب.")]
            public QuestionType QuestionType { get; set; }

            //[Required]
            //[MinLength(2, ErrorMessage = "يجب توفير خيارين على الأقل.")]
            public List<CreateQuizQuestionOptionDto> Options { get; set; } = new List<CreateQuizQuestionOptionDto>();
        }
    }