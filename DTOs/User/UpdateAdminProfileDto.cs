using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.User
{
    public class UpdateAdminProfileDto
    {
        [Required(ErrorMessage = "الرقم الوطني الحالي مطلوب للتحقق.")]
        public string CurrentNationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "الاسم الأول مطلوب.")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم العائلة مطلوب.")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم الوطني مطلوب.")]
        [StringLength(50)]
        public string NationalId { get; set; } = string.Empty;
    }
}