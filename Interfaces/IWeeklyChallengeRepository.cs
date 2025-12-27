 
using SmartSchoolAPI.Entities;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    public interface IWeeklyChallengeRepository
    {
        /// <summary>
        /// يبحث عن تقديم موجود لطالب معين في مساق معين لأسبوع وسنة محددين.
        /// </summary>
        Task<WeeklyChallengeSubmission?> GetSubmissionAsync(int studentId, int courseId, int year, int weekOfYear);

        /// <summary>
        /// يضيف تقديمًا جديدًا إلى قاعدة البيانات.
        /// </summary>
        Task AddSubmissionAsync(WeeklyChallengeSubmission submission);


        /// <summary>
        /// يجلب قائمة بتقديمات التحدي لـ course/year/week معين، مرتبة لعرضها في لوحة الصدارة.
        /// </summary>
        Task<List<WeeklyChallengeSubmission>> GetLeaderboardAsync(int courseId, int year, int weekOfYear);
    }
}
