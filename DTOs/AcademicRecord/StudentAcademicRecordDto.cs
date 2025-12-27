using SmartSchoolAPI.Enums;

namespace SmartSchoolAPI.DTOs.AcademicRecord
{
    public class StudentAcademicRecordDto
    {
        public AcademicStatus OverallStatus { get; set; }
        public decimal? FinalGpa { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? ProgramNameAtCompletion { get; set; }
        public List<EnrollmentRecordDto> EnrollmentHistory { get; set; } = new List<EnrollmentRecordDto>();
    }
}
