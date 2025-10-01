using SmartSchoolAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// واجهة لعمليات الوصول لبيانات البرامج الأكاديمية.
    /// </summary>
    public interface IAcademicProgramRepository
    {
        #region عمليات القراءة (Read Operations)

        Task<AcademicProgram?> GetProgramByIdAsync(int programId);
        Task<IEnumerable<AcademicProgram>> GetAllProgramsAsync();

        Task<IEnumerable<AcademicProgram>> GetProgramsOpenForRegistrationAsync();


 

        #endregion

        #region عمليات الكتابة (Write Operations)

        Task CreateProgramAsync(AcademicProgram program);
        void UpdateProgram(AcademicProgram program);
        void DeleteProgram(AcademicProgram program);

        #endregion

        #region إدارة حفظ البيانات

        Task<bool> SaveChangesAsync();

        #endregion
    }
}