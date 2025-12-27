namespace SmartSchoolAPI.DTOs.Certificate
{
    public class CertificateVerificationDto
    {
        public string FullName { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public DateTime GraduationDate { get; set; }
        public decimal? FinalGpa { get; set; }
        public string CertificateDownloadUrl { get; set; } = string.Empty;
    }
}