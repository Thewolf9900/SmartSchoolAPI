namespace SmartSchoolAPI.DTOs.QuestionBank
{
    public class QuestionOptionDto
    {
        public int QuestionOptionId { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}