namespace SmartSchoolAPI.DTOs.Quiz
{
    public class QuizQuestionOptionDto
    {
        public int LectureQuizQuestionOptionId { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}