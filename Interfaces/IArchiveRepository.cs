using SmartSchoolAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// واجهة لعمليات الوصول لبيانات الأرشفة.
    /// </summary>
    public interface IArchiveRepository
    {
        #region عمليات القراءة

        Task<IEnumerable<ArchivedClassroom>> GetAllArchivedClassroomsAsync();

        #endregion

        #region عمليات الكتابة

        Task CreateArchivedClassroomAsync(ArchivedClassroom archivedClassroom);

        #endregion

        #region إدارة حفظ البيانات

        Task<bool> SaveChangesAsync();

        #endregion
    }
}