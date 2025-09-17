using SmartSchoolAPI.DTOs.Announcement;
using System.Collections.Generic;

namespace SmartSchoolAPI.DTOs.Classroom
{
    public class ClassroomDetailsDto
    {
        public int ClassroomId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public int EnrolledStudentsCount { get; set; }
        public int Capacity { get; set; }
        public int LectureCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<SimpleAnnouncementDto> Announcements { get; set; } = new List<SimpleAnnouncementDto>();
    }
}