using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.User
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required.")]
        public required string OldPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters long.")]
        public required string NewPassword { get; set; }
    }
}