namespace SmartSchoolAPI.DTOs.Quiz
{
    public class LectureQuizSummaryDto
    {
        public int LectureQuizId { get; set; }
        public string Title { get; set; }
        public bool IsEnabled { get;  set; }
        public bool IsSubmitted { get; set; }
        public int? SubmissionId { get; set; }

    }
}