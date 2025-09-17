using SmartSchoolAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// واجهة لعمليات الوصول لبيانات التخرج والرسوب.
    /// </summary>
    public interface IGraduationRepository
    {
        #region عمليات قراءة سجلات الخريجين

        Task<Graduation?> GetGraduationByIdAsync(int graduationId);
        Task<Graduation?> GetGraduationByStudentIdAsync(int studentId);
        Task<IEnumerable<Graduation>> GetGraduationsByNationalIdAsync(string nationalId);
        Task<Graduation?> GetGraduationRecordAsync(int studentId, int programId);
        Task<IEnumerable<Graduation>> GetGraduatesAsync(int? programId, int? year, int? month);
        Task<IEnumerable<Graduation>> GetGraduatesPendingCertificateAsync();

        #endregion

        #region عمليات قراءة سجلات الراسبين

        Task<FailedStudent?> GetFailureByStudentIdAsync(int studentId);
        Task<IEnumerable<FailedStudent>> GetFailuresAsync(int? programId, int? year, int? month);

        #endregion

        #region عمليات التحقق

        Task<bool> HasAlreadyGraduatedAsync(int studentId, int programId);
        Task<bool> HasFailedAsync(int studentId, int programId);

        #endregion

        #region عمليات الكتابة (الإنشاء والحذف)

        Task CreateGraduationRecordAsync(Graduation graduation);
        Task CreateFailedStudentRecordAsync(FailedStudent failedStudent);
        Task AddCertificateAsync(GraduationCertificate certificate);
        Task DeleteCertificateAsync(int graduationId);

        #endregion

        #region إدارة حفظ البيانات

        Task<bool> SaveChangesAsync();

        #endregion
    }
}