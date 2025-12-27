using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// واجهة لعمليات الوصول لبيانات المستخدمين (طلاب، مدرسين، مديرين).
    /// </summary>
    public interface IUserRepository
    {
        #region عمليات القراءة العامة

        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByNationalIdAsync(string nationalId);
        Task<IEnumerable<User>> GetAllUsersAsync(UserRole? role);
        Task<User?> GetUserWithProgramByIdAsync(int userId);

        #endregion

        #region عمليات القراءة الخاصة بالطلاب

        Task<User?> GetStudentProfileByIdAsync(int studentId);
        Task<IEnumerable<User>> GetActiveStudentsAsync();
        Task<IEnumerable<User>> GetUnassignedStudentsAsync();
        Task<IEnumerable<User>> GetStudentsByProgramAsync(int programId);
        Task<IEnumerable<User>> GetAvailableStudentsForCourseAsync(int courseId);
        Task<IEnumerable<User>> GetStudentsWithEnrollmentsByProgramAsync(int programId);

        #endregion

        #region عمليات القراءة الخاصة بالمدرسين

        Task<IEnumerable<User>> GetTeachersByCourseAsync(int courseId);

        #endregion

        #region عمليات الكتابة وإدارة الحساب

        Task CreateUserAsync(User user);
        void DeleteUser(User user);
        Task<PasswordResetToken?> GetPasswordResetTokenByUserIdAsync(int userId);
        Task SetPasswordResetTokenAsync(PasswordResetToken token);

        #endregion

        #region إدارة حفظ البيانات

        Task<bool> SaveChangesAsync();

        #endregion
    }
}