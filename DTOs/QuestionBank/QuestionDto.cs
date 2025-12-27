using SmartSchoolAPI.Enums;
using System;
using System.Collections.Generic;

namespace SmartSchoolAPI.DTOs.QuestionBank
{
    public class QuestionDto
    {
        public int QuestionId { get; set; }
        public string? Text { get; set; }
        public string? ImageUrl { get; set; }
        public QuestionType QuestionType { get; set; }
        public DifficultyLevel DifficultyLevel { get; set; }
        public QuestionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? ReviewedBy { get; set; }
        public List<QuestionOptionDto> Options { get; set; } = new List<QuestionOptionDto>();
    }
}