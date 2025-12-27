namespace SmartSchoolAPI.DTOs.Reports
{
    public class StudentDeficiencyDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public List<CourseShortDto> MissingCourses { get; set; } = new List<CourseShortDto>();
    }
}
