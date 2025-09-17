using SmartSchoolAPI.DTOs.QuestionBank;
using SmartSchoolAPI.Entities;
using System.Collections.Generic; // Required for IEnumerable
using System.Threading.Tasks; // Required for Task

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// يُعرّف العقد الخاص بالخدمات المسؤولة عن التفاعل مع نماذج الذكاء الاصطناعي.
    /// </summary>
    public interface IAiService
    {
        /// <summary>
        /// يولد أسئلة بناءً على محتوى نصي وإعدادات محددة.
        /// </summary>
        /// <param name="generationParams">كائن يحتوي على المحتوى النصي والإعدادات الأخرى.</param>
        /// <param name="language">اللغة المطلوبة لمخرجات الأسئلة (e.g., "Arabic", "English").</param>
        /// <returns>مهمة تمثل قائمة من كائنات الأسئلة المقترحة.</returns>
        Task<IEnumerable<CreateQuestionDto>> GenerateQuestionsAsync(GenerateQuestionsFromTextDto generationParams, string language);

        Task<string> GetChatResponseAsync(IEnumerable<ChatMessage> messageHistory);

    }
}