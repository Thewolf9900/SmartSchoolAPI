using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// واجهة لخدمة إرسال رسائل البريد الإلكتروني.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// يرسل بريدًا إلكترونيًا إلى مستلم محدد.
        /// </summary>
        /// <param name="toEmail">عنوان البريد الإلكتروني للمستلم.</param>
        /// <param name="subject">موضوع الرسالة.</param>
        /// <param name="body">محتوى الرسالة (يمكن أن يكون HTML).</param>
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}