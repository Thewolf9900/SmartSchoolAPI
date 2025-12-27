namespace SmartSchoolAPI.DTOs.AcademicProgram
{
    /// <summary>
    /// نموذج لنقل البيانات المستخدم في عرض تفاصيل برنامج أكاديمي.
    /// </summary>
    public class ProgramDto
    {
      
        public int AcademicProgramId { get; set; }
 
        public string Name { get; set; } = string.Empty;
      
        public string? Description { get; set; }

        public bool IsRegistrationOpen { get; set; } = false;

    }
}