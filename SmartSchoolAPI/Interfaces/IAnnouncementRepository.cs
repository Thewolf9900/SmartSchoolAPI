using SmartSchoolAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// واجهة لعمليات الوصول لبيانات الإعلانات.
    /// </summary>
    public interface IAnnouncementRepository
    {
        #region عمليات القراءة العامة والخاصة بالمدير

        Task<Announcement?> GetAnnouncementByIdAsync(int announcementId);
        Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync();

        #endregion

        #region عمليات القراءة الخاصة بالمدرس

        Task<IEnumerable<Announcement>> GetLatestAnnouncementsForClassroomAsync(int classroomId, int count);

        #endregion

        #region عمليات القراءة الخاصة بالطالب

        Task<IEnumerable<Announcement>> GetAnnouncementsForStudentAsync(int programId, List<int> courseIds, List<int> classroomIds);

        #endregion

        #region عمليات الكتابة

        Task CreateAnnouncementAsync(Announcement announcement);
        void DeleteAnnouncement(Announcement announcement);

        #endregion

        #region إدارة حفظ البيانات

        Task<bool> SaveChangesAsync();

        #endregion
    }
}