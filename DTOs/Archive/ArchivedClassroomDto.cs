using System;
using System.Collections.Generic;

namespace SmartSchoolAPI.DTOs.Archive
{
    public class ArchivedClassroomDto
    {
        public int ArchivedClassroomId { get; set; }
        public int OriginalClassroomId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public string? TeacherName { get; set; }
        public DateTime ArchivedAt { get; set; }
        public List<ArchivedEnrollmentDto> EnrolledStudents { get; set; } = new List<ArchivedEnrollmentDto>();
    }
}