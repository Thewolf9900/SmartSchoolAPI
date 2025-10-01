using SmartSchoolAPI.Enums;

namespace SmartSchoolAPI.DTOs.Auth
{
    public class CheckStatusResponseDto
    {
        public string UserType { get; set; } // e.g., "Student", "Teacher", "Admin", "Applicant"
        public string? Token { get; set; }
        public RegistrationStatus? ApplicantStatus { get; set; }
        public int? RegistrationId { get; set; }
        public string FullName { get; set; }
    }
}