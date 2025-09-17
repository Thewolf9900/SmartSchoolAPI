using SmartSchoolAPI.Enums;
using SmartSchoolAPI.DTOs.Lecture;
using System.Collections.Generic;

namespace SmartSchoolAPI.DTOs.Classroom
{
    public class StudentClassroomDetailsDto
    {
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
        public int CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;
        public string? TeacherName { get; set; }
        public ClassroomStatus Status { get; set; }
        public decimal? PracticalGrade { get; set; }
        public decimal? ExamGrade { get; set; }
        public decimal? FinalGrade { get; set; }
        public List<LectureContentDto> Lectures { get; set; } = new List<LectureContentDto>();
    }
}