using System;

namespace SmartSchoolAPI.DTOs.Announcement
{
    public class SimpleAnnouncementDto
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
    }
}