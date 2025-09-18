using Microsoft.AspNetCore.Http;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// يُعرّف العقد الخاص بالخدمات المسؤولة عن التعامل مع نظام الملفات،
    /// بما في ذلك الحفظ، الحذف، واستخلاص المحتوى.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// يقرأ محتوى ملف بشكل غير متزامن ويستخلص منه نصًا.
        /// يمكن أن يتضمن التنفيذ منطقًا متقدمًا للتعامل مع أنواع ملفات مختلفة مثل PDF.
        /// </summary>
        /// <param name="file">الملف المراد قراءته.</param>
        /// <returns>مهمة تمثل سلسلة نصية تحتوي على محتوى الملف أو عينة منه.</returns>
        Task<string> ReadFileContentAsync(IFormFile file);

        /// <summary>
        /// يحفظ ملفًا في الخادم بشكل غير متزامن ضمن مجلد فرعي محدد.
        /// </summary>
        /// <param name="file">الملف المراد حفظه.</param>
        /// <param name="subfolder">اسم المجلد الفرعي داخل مجلد 'uploads'.</param>
        /// <returns>مهمة تمثل المسار النسبي للملف المحفوظ.</returns>
        Task<string> SaveFileAsync(IFormFile file, string subfolder);

        /// <summary>
        /// يحذف ملفًا ماديًا من الخادم بناءً على مساره النسبي.
        /// </summary>
        /// <param name="relativePath">المسار النسبي للملف (e.g., 'uploads/folder/file.jpg').</param>
        Task DeleteFileAsync(string fileUrl);

        /// <summary>
        /// يسترجع بيانات ملف مادي (بايتات، نوع المحتوى، اسم الملف) من الخادم.
        /// </summary>
        /// <param name="relativePath">المسار النسبي للملف.</param>
        /// <returns>Tuple يحتوي على بيانات الملف أو قيم null إذا لم يتم العثور عليه.</returns>
        (byte[] fileBytes, string contentType, string fileName) GetPhysicalFile(string relativePath);
    }
}