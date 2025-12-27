
using System;
using System.Collections.Generic;

namespace SmartSchoolAPI.DTOs.Quiz
{
    public class QuizReviewDetailsDto
    {
        public int SubmissionId { get; set; }
        public string? QuizTitle { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime SubmittedAt { get; set; }
        public List<QuestionReviewDto> QuestionReviews { get; set; }
    }
}