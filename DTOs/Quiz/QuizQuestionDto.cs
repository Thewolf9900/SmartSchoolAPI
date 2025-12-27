using SmartSchoolAPI.Enums;
using System.Collections.Generic;

namespace SmartSchoolAPI.DTOs.Quiz
{
    public class QuizQuestionDto
    {
        public int LectureQuizQuestionId { get; set; }
        public string? Text { get; set; } 
        public string? ImageUrl { get; set; }  
        public QuestionType QuestionType { get; set; }
        public List<QuizQuestionOptionDto> Options { get; set; } = new List<QuizQuestionOptionDto>();
    }
}