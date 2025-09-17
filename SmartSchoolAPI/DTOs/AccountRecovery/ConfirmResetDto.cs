using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.AccountRecovery
{
    public class ConfirmResetDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز إعادة التعيين مطلوب.")]
        public string ResetCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "يجب أن تتكون كلمة المرور من 8 أحرف على الأقل.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}