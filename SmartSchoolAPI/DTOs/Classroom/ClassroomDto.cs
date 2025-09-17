using SmartSchoolAPI.Enums;

namespace SmartSchoolAPI.DTOs.Classroom
{
    public class ClassroomDto
    {
        public int ClassroomId { get; set; }
        public string Name { get; set; } = string.Empty;

        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;

         public int AcademicProgramId { get; set; }
        public string AcademicProgramName { get; set; } = string.Empty;
 
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; } = string.Empty;
        public int EnrolledStudentsCount { get; set; }
    }
}
