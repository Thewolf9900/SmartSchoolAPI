using System;

namespace SmartSchoolAPI.DTOs.Quiz
{
    public class QuizSubmissionSummaryDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}