using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.User
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "الرقم الوطني مطلوب.")]
        public string NationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "يجب أن تتكون كلمة المرور من 8 أحرف على الأقل.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}