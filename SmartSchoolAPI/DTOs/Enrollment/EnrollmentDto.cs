namespace SmartSchoolAPI.DTOs.Enrollment
{
    public class EnrollmentDto
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
         public DateTime EnrollmentDate { get; set; }
        public decimal? PracticalGrade { get; set; } 
        public decimal? ExamGrade { get; set; }
        public decimal? FinalGrade { get; set; }
    }
}
