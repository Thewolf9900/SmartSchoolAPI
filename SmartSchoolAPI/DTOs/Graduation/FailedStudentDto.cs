namespace SmartSchoolAPI.DTOs.Graduation
{
     public class FailedStudentDto
    {
        public int FailureId { get; set; }
        public int? StudentUserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    
        public string NationalId { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public DateTime FailureDate { get; set; }
        public decimal FinalGpa { get; set; }
        public string? Notes { get; set; }
    }
}
