namespace SmartSchoolAPI.DTOs.AcademicRecord
{
    public class EnrollmentRecordDto
    {
        public string CourseName { get; set; } = string.Empty;
        public string ClassroomName { get; set; } = string.Empty;
        public decimal? PracticalGrade { get; set; }
        public decimal? ExamGrade { get; set; }
        public decimal? FinalGrade { get; set; }
        public string ClassroomStatus { get; set; } = string.Empty;
    }
}
