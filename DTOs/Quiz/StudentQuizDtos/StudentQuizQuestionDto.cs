namespace SmartSchoolAPI.DTOs.Quiz
{
    public class StudentQuizQuestionDto
{
    public int LectureQuizQuestionId { get; set; }
    public string Text { get; set; }
    public string ImageUrl { get; set; }
    public List<StudentQuizOptionDto> Options { get; set; }
}}