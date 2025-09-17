namespace SmartSchoolAPI.DTOs.Quiz
{
    public class QuizResultDto
    {
        public int SubmissionId { get; set; }
        public int StudentId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public string Message { get; set; }
        public List<QuestionReviewDto> QuestionReviews { get; set; }

    }
}