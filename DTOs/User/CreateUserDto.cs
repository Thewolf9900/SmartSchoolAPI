using SmartSchoolAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.User
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب.")]
        [StringLength(100, ErrorMessage = "يجب ألا يتجاوز الاسم الأول 100 حرف.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم العائلة مطلوب.")]
        [StringLength(100, ErrorMessage = "يجب ألا يتجاوز اسم العائلة 100 حرف.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "يجب أن تتكون كلمة المرور من 8 أحرف على الأقل.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم الوطني مطلوب.")]
        [StringLength(50, ErrorMessage = "يجب ألا يتجاوز الرقم الوطني 50 حرفًا.")]
        public string NationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "يجب تحديد دور المستخدم.")]
        [EnumDataType(typeof(UserRole), ErrorMessage = "قيمة الدور غير صالحة.")]
        public UserRole Role { get; set; }
    }
}