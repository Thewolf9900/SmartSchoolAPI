namespace SmartSchoolAPI.DTOs.Course
{
    public class CourseDto
    {
        public int CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }  
        public int AcademicProgramId { get; set; }
        public string AcademicProgramName { get; set; } = string.Empty;
        public int? CoordinatorId { get; set; }
        public string CoordinatorName { get; set; } = "غير معين";
    }
}