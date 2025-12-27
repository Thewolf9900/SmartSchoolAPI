using SmartSchoolAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// واجهة لعمليات الوصول لبيانات الدورات الدراسية.
    /// </summary>
    public interface ICourseRepository
    {
        #region عمليات القراءة

        Task<Course?> GetCourseByIdAsync(int courseId);
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        Task<IEnumerable<Course>> GetCoursesByProgramAsync(int academicProgramId);
        Task<IEnumerable<Course>> GetCoursesWithoutClassroomsAsync();
        Task<IEnumerable<Course>> GetCoursesWithoutCoordinatorAsync();
        Task<IEnumerable<Course>> GetCoursesByCoordinatorIdAsync(int coordinatorId);

        #endregion

        #region عمليات الكتابة

        Task CreateCourseAsync(Course course);
        void UpdateCourse(Course course);
        void DeleteCourse(Course course);
        Task<(bool Success, string ErrorMessage)> AssignCoordinatorAsync(int courseId, int teacherId);
        Task<bool> UnassignCoordinatorAsync(int courseId);


        #endregion

        #region إدارة حفظ البيانات

        Task<bool> SaveChangesAsync();

        #endregion
    }
}