namespace SmartSchoolAPI.DTOs.Reports
{
    public class EnrollmentDeficiencyReportDto
    {
        public int ProgramId { get; set; }
        public string ProgramName { get; set; } = string.Empty;
        public List<StudentDeficiencyDto> StudentsWithDeficiencies { get; set; } = new List<StudentDeficiencyDto>();
    }
}
