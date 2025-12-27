using Microsoft.AspNetCore.Http;
using SmartSchoolAPI.DTOs; // تأكد من إنشاء DTOs/FileUploadResult.cs أولاً
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    /// <summary>
    /// يُعرّف العقد الخاص بالخدمات المسؤولة عن التعامل مع الملفات،
    /// مع التركيز على عمليات التخزين السحابي (Cloudinary).
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// يقرأ محتوى ملف بشكل غير متزامن ويستخلص منه نصًا للتحليل.
        /// </summary>
        /// <param name="file">الملف المراد قراءته.</param>
        /// <returns>مهمة تمثل سلسلة نصية تحتوي على محتوى الملف أو عينة منه.</returns>
        Task<string> ReadFileContentAsync(IFormFile file);

        /// <summary>
        /// يرفع ملفًا إلى خدمة التخزين السحابي ويعيد تفاصيله.
        /// </summary>
        /// <param name="file">الملف المراد رفعه.</param>
        /// <param name="subfolder">المجلد الفرعي داخل Cloudinary لتنظيم الملفات.</param>
        /// <returns>مهمة تمثل كائنًا يحتوي على رابط الملف العام (Url) والمعرف الفريد (PublicId).</returns>
        Task<FileUploadResult> SaveFileAsync(IFormFile file, string subfolder);

        /// <summary>
        /// يحذف ملفًا من خدمة التخزين السحابي باستخدام المعرف الفريد الخاص به.
        /// </summary>
        /// <param name="publicId">المعرف الفريد (PublicId) للملف في Cloudinary.</param>
        Task DeleteFileAsync(string publicId);

        
    }
}