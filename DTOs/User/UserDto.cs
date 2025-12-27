using SmartSchoolAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.User
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }=string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public string? AcademicProgramName { get; set; }

    }
}
