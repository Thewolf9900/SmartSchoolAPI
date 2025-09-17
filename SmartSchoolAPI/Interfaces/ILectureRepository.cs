using SmartSchoolAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// واجهة لعمليات الوصول لبيانات المحاضرات.
    /// </summary>
    public interface ILectureRepository
    {
        #region عمليات القراءة

        Task<Lecture?> GetLectureByIdAsync(int lectureId);
        Task<IEnumerable<Lecture>> GetLecturesByClassroomIdAsync(int classroomId);
        Task<Lecture?> GetLectureWithMaterialsByIdAsync(int lectureId);
        Task<IEnumerable<Lecture>> GetLecturesWithMaterialsAsync(int classroomId);
        Task<Classroom?> GetClassroomForLectureAsync(int lectureId);
        #endregion

        #region عمليات الكتابة

        void CreateLecture(Lecture lecture);
        void UpdateLecture(Lecture lecture);
        void DeleteLecture(Lecture lecture);

        #endregion

        #region إدارة حفظ البيانات

        Task<bool> SaveChangesAsync();

        #endregion
    }
}