namespace SmartSchoolAPI.DTOs.Archive
{
    public class ArchivedEnrollmentDto
    {
        public string StudentName { get; set; } = string.Empty;
        public string? StudentNationalId { get; set; }
        public decimal? PracticalGrade { get; set; } 
        public decimal? ExamGrade { get; set; }       
        public decimal? FinalGrade { get; set; }
    }
}