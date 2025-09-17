using SmartSchoolAPI.DTOs.Enrollment;
using SmartSchoolAPI.DTOs.Reports;
using SmartSchoolAPI.DTOs.User;
using System.Collections.Generic;

namespace SmartSchoolAPI.DTOs.Profile
{
    public class StudentProfileDto
    {
        public UserDto UserInfo { get; set; }= new UserDto();
        public string ProgramName { get; set; } = string.Empty;
        public List<EnrollmentDto> Enrollments { get; set; } =new List<EnrollmentDto>();
        public List<CourseShortDto> MissingCourses { get; set; } = new List<CourseShortDto>();

    }
}