using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.AccountRecovery
{
    public class FindEmailDto
    {
        [Required(ErrorMessage = "الرقم الوطني مطلوب.")]
        public string NationalId { get; set; } = string.Empty;
    }
}