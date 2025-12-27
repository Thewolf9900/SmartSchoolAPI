using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// واجهة لعمليات الوصول لبيانات الفصول الدراسية.
    /// </summary>
    public interface IClassroomRepository
    {
        #region عمليات القراءة العامة والخاصة بالمدير

        Task<Classroom?> GetClassroomByIdAsync(int classroomId);
        Task<IEnumerable<Classroom>> GetAllClassroomsAsync(ClassroomStatus? status);
        Task<IEnumerable<Classroom>> GetClassroomsByCourseAsync(int courseId);
        Task<IEnumerable<Classroom>> GetClassroomsWithoutTeacherAsync();

        #endregion

        #region عمليات القراءة الخاصة بالمدرس

        Task<IEnumerable<Classroom>> GetClassroomsByTeacherIdAsync(int teacherId, ClassroomStatus? status);
        Task<Classroom?> GetClassroomWithDetailsForTeacherPortalAsync(int classroomId);
        Task<IEnumerable<Classroom>> GetClassroomsByTeacherAsync(int teacherId);
        Task<bool> IsTeacherAssociatedWithCourseAsync(int teacherId, int courseId);


        #endregion

        #region عمليات القراءة الخاصة بالطالب

        Task<Classroom?> GetClassroomDetailsForStudentAsync(int classroomId);

        #endregion

        #region عمليات الكتابة

        Task CreateClassroomAsync(Classroom classroom);
        void UpdateClassroom(Classroom classroom);
        void DeleteClassroom(Classroom classroom);

        #endregion

        #region إدارة حفظ البيانات

        Task<bool> SaveChangesAsync();

        #endregion
    }
}