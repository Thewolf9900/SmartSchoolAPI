using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// واجهة لخدمات منطق العمل المتعلقة بعمليات الأرشفة.
    /// </summary>
    public interface IArchiveService
    {
        /// <summary>
        /// يقوم بأرشفة فصل دراسي معين، بما في ذلك كل التسجيلات والبيانات المرتبطة به.
        /// </summary>
        /// <param name="classroomId">المعرف الفريد للفصل الدراسي المراد أرشفته.</param>
        /// <returns>قيمة منطقية تشير إلى نجاح العملية.</returns>
        Task<(bool Success, string Message)> ArchiveClassroomAsync(int classroomId);
    }
}