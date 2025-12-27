namespace SmartSchoolAPI.DTOs.Quiz
{
    public class StudentQuizDto
{
    public int LectureQuizId { get; set; }
    public string Title { get; set; }
    public List<StudentQuizQuestionDto> Questions { get; set; }
}
}