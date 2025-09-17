using SmartSchoolAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Announcement
{
    public class CreateAnnouncementDto
    {
        [Required(ErrorMessage = "عنوان الإعلان مطلوب.")]
        [StringLength(255, ErrorMessage = "يجب ألا يتجاوز العنوان 255 حرفًا.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "محتوى الإعلان مطلوب.")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "يجب تحديد نطاق الإعلان.")]
        public AnnouncementScope TargetScope { get; set; }

        public int? TargetId { get; set; }
    }
}