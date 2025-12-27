using System;

namespace SmartSchoolAPI.DTOs.Alumni
{
    public class AlumniRecordDto
    {
        public int GraduationId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ProgramNameAtGraduation { get; set; } = string.Empty;
        public DateTime GraduationDate { get; set; }
        public decimal? FinalGpa { get; set; }
        public bool HasCertificate { get; set; }
    }
}