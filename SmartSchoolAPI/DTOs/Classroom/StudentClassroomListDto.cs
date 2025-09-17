using SmartSchoolAPI.Enums;

namespace SmartSchoolAPI.DTOs.Classroom
{
    public class StudentClassroomListDto
    {
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? TeacherName { get; set; }
        public ClassroomStatus Status { get; set; }
        public decimal? FinalGrade { get; set; }

    }
}