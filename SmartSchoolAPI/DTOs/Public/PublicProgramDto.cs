using System.Collections.Generic;

namespace SmartSchoolAPI.DTOs.Public
{
    public class PublicProgramDto
    {
        public int AcademicProgramId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal TotalPrice { get; set; }
        public List<string> CourseNames { get; set; } = new List<string>();
    }
}