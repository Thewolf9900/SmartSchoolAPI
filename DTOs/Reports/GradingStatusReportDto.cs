namespace SmartSchoolAPI.DTOs.Reports
{
    public class GradingStatusReportDto
    {
        public int TotalStudents { get; set; }
        public int GradesEnteredCount { get; set; }
        public int GradesMissingCount { get; set; }

         public bool IsComplete { get; set; }

         public List<StudentInfoForReportDto> MissingGradesForStudents { get; set; } = new List<StudentInfoForReportDto>();
    }
}
