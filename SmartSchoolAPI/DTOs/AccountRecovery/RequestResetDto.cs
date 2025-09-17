using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.AccountRecovery
{
    public class RequestResetDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم الوطني مطلوب.")]
        public string NationalId { get; set; } = string.Empty;
    }
}