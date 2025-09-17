
public class QuestionReviewDto
{
    public int QuestionId { get; set; }
    public string? QuestionText { get; set; }
    public string? ImageUrl { get; set; }

    public string? YourAnswerText { get; set; }
    public int YourAnswerOptionId { get; set; }
    public string CorrectAnswerText { get; set; }
    public bool WasCorrect { get; set; }
}