using SmartSchoolAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// واجهة لعمليات الوصول لبيانات المواد التعليمية (المرجعية ومواد المحاضرات).
    /// </summary>
    public interface IMaterialRepository
    {
        #region عمليات القراءة

        Task<Material?> GetMaterialByIdAsync(int materialId);
        Task<IEnumerable<Material>> GetMaterialsForCourseAsync(int courseId);
        Task<Material?> GetMaterialForStudentDownloadAsync(int materialId);
        Task<Material?> GetMaterialWithDeepDetailsAsync(int materialId);

        #endregion

        #region عمليات الكتابة

        Task CreateMaterialAsync(Material material);
        void UpdateMaterial(Material material);
        void DeleteMaterial(Material material);

        #endregion

        #region إدارة حفظ البيانات

        Task<bool> SaveChangesAsync();

        #endregion
    }
}