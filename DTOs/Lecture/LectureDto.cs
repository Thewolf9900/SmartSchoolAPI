namespace SmartSchoolAPI.DTOs.Lecture
{
    public class LectureDto
    {
        public int LectureId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int LectureOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
