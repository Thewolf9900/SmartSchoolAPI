using SmartSchoolAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// يُعرّف العقد الخاص بعمليات الوصول إلى بيانات المحادثات والرسائل.
    /// </summary>
    public interface IChatRepository
    {
        /// <summary>
        /// يجلب كل المحادثات الخاصة بمستخدم معين، بدون تحميل الرسائل.
        /// </summary>
        /// <param name="userId">معرف المستخدم.</param>
        /// <returns>قائمة من المحادثات.</returns>
        Task<IEnumerable<ChatConversation>> GetConversationsByUserIdAsync(int userId);

        /// <summary>
        /// يجلب محادثة معينة بكل رسائلها.
        /// </summary>
        /// <param name="conversationId">معرف المحادثة.</param>
        /// <param name="userId">معرف المستخدم للتحقق من الملكية.</param>
        /// <returns>كائن المحادثة مع رسائلها، أو null إذا لم يتم العثور عليها أو لم يكن المستخدم يملكها.</returns>
        Task<ChatConversation> GetConversationWithMessagesAsync(int conversationId, int userId);

        /// <summary>
        /// يضيف محادثة جديدة إلى قاعدة البيانات.
        /// </summary>
        /// <param name="conversation">كائن المحادثة المراد إضافته.</param>
        Task AddConversationAsync(ChatConversation conversation);

        /// <summary>
        /// يضيف رسالة جديدة إلى محادثة موجودة.
        /// </summary>
        /// <param name="message">كائن الرسالة المراد إضافته.</param>
        Task AddMessageAsync(ChatMessage message);

        /// <summary>
        /// يحذف محادثة بالكامل مع جميع رسائلها.
        /// </summary>
        /// <param name="conversation">كائن المحادثة المراد حذفها.</param>
        void DeleteConversation(ChatConversation conversation);

        /// <summary>
        /// يمسح كل الرسائل من محادثة معينة.
        /// </summary>
        /// <param name="conversationId">معرف المحادثة المراد مسحها.</param>
        Task ClearConversationMessagesAsync(int conversationId);

        /// <summary>
        /// يحفظ كل التغييرات المعلقة في قاعدة البيانات.
        /// </summary>
        Task<bool> SaveChangesAsync();
    }
}