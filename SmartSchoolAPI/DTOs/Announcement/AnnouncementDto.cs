using SmartSchoolAPI.Enums;
using System;

namespace SmartSchoolAPI.DTOs.Announcement
{
    public class AnnouncementDto
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
        public AnnouncementScope TargetScope { get; set; }
        public int? TargetId { get; set; }
        public string? TargetName { get; set; }
    }
}