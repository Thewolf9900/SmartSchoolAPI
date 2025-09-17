using SmartSchoolAPI.DTOs.Material;
using SmartSchoolAPI.DTOs.Quiz;

namespace SmartSchoolAPI.DTOs.Lecture
{
    public class LectureContentDto
    {
        public int LectureId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int LectureOrder { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<MaterialDto> Materials { get; set; } = new List<MaterialDto>();

        public LectureQuizSummaryDto? LectureQuiz { get; set; }
    }
}
