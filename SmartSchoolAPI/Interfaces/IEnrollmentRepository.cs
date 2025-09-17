using SmartSchoolAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// واجهة لعمليات الوصول لبيانات تسجيلات الطلاب في الفصول.
    /// </summary>
    public interface IEnrollmentRepository
    {
        #region عمليات القراءة

        Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId);
        Task<Enrollment?> GetEnrollmentWithDetailsByIdAsync(int enrollmentId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsForClassroomAsync(int classroomId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsForStudentAsync(int studentId);
        Task<bool> IsStudentEnrolledAsync(int studentId, int classroomId);
        Task<bool> IsStudentEnrolledInCourseAsync(int studentId, int courseId);

        #endregion

        #region عمليات الكتابة

        Task CreateEnrollmentAsync(Enrollment enrollment);
        void DeleteEnrollment(Enrollment enrollment);

        #endregion

        #region إدارة حفظ البيانات

        Task<bool> SaveChangesAsync();

        #endregion
    }
}