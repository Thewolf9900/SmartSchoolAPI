namespace SmartSchoolAPI.DTOs.Graduation
{
    public class GraduateDto
    {
        public int GraduationId { get; set; }
        public int? StudentUserId { get; set; }  
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public DateTime GraduationDate { get; set; }
        public decimal? FinalGpa { get; set; }
        public bool HasCertificate { get; set; }
    }
}
