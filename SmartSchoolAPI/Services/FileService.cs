using Microsoft.AspNetCore.StaticFiles;
using SmartSchoolAPI.Interfaces;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SmartSchoolAPI.Services
{
    /// <summary>
    /// خدمة مسؤولة عن كافة عمليات التعامل مع الملفات المادية،
    /// بما في ذلك الحفظ، الحذف، واستخلاص المحتوى النصي بشكل متقدم.
    /// </summary>
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        public FileService(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        #region استخلاص المحتوى للذكاء الاصطناعي

        /// <summary>
        /// يقرأ محتوى ملف ويستخلص منه نصًا جاهزًا للمعالجة بواسطة الذكاء الاصطناعي.
        /// - للملفات النصية: يقرأ المحتوى بالكامل.
        /// - لملفات PDF: يأخذ عينة عشوائية من الصفحات لضمان عدم تجاوز حدود الإدخال.
        /// </summary>
        /// <param name="file">الملف المرفوع من نوع IFormFile.</param>
        /// <returns>سلسلة نصية تمثل عينة من محتوى الملف.</returns>
        public async Task<string> ReadFileContentAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            // إذا لم يكن الملف PDF، يتم التعامل معه كملف نصي عادي.
            if (fileExtension != ".pdf")
            {
                using var reader = new StreamReader(file.OpenReadStream());
                return await reader.ReadToEndAsync();
            }

            // --- تطبيق منطق العينات العشوائية الذكي لملفات PDF ---

            // تحديد معايير العينة لضمان أداء متسق.
            const int numberOfPagesToSample = 5; // عدد الصفحات الأقصى للعينة.
            const int maxCharacterLimit = 2500;  // الحد الأقصى لعدد الأحرف في النص النهائي.

            var allPagesText = new List<string>();

            // الخطوة 1: استخلاص النص من كل صفحة وتخزينها في قائمة.
            using (var stream = file.OpenReadStream())
            {
                using PdfDocument document = PdfDocument.Open(stream);
                if (document.NumberOfPages == 0) return string.Empty;

                foreach (Page page in document.GetPages())
                {
                    if (!string.IsNullOrWhiteSpace(page.Text))
                    {
                        allPagesText.Add(page.Text);
                    }
                }
            }

            // الخطوة 2: اختيار عينة عشوائية من الصفحات المستخلصة.
            var random = new Random();
            int pagesToTake = Math.Min(numberOfPagesToSample, allPagesText.Count);

            // يتم خلط قائمة الصفحات لضمان العشوائية ثم اختيار العدد المطلوب.
            var sampledPages = allPagesText.OrderBy(x => random.Next()).Take(pagesToTake).ToList();

            // الخطوة 3: تجميع نصوص العينة مع مراقبة الحد الأقصى للحجم.
            var textBuilder = new StringBuilder();
            foreach (var pageText in sampledPages)
            {
                // إذا كانت إضافة نص الصفحة التالية ستتجاوز الحد، يتم التوقف.
                if (textBuilder.Length + pageText.Length > maxCharacterLimit)
                {
                    int remainingChars = maxCharacterLimit - textBuilder.Length;
                    if (remainingChars > 0)
                    {
                        // إضافة جزء من النص فقط لملء المساحة المتبقية.
                        textBuilder.Append(pageText.Substring(0, Math.Min(pageText.Length, remainingChars)));
                    }
                    break;
                }

                textBuilder.AppendLine(pageText);
            }

            // الخطوة 4: إجراء قص نهائي كإجراء احترازي لضمان عدم تجاوز الحد الأقصى.
            if (textBuilder.Length > maxCharacterLimit)
            {
                textBuilder.Length = maxCharacterLimit;
            }

            return textBuilder.ToString();
        }

        #endregion

        #region عمليات نظام الملفات

        /// <summary>
        /// يحفظ ملفًا في الخادم ضمن مجلد فرعي محدد.
        /// </summary>
        /// <param name="file">الملف المراد حفظه.</param>
        /// <param name="subfolder">اسم المجلد الفرعي داخل مجلد 'uploads'.</param>
        /// <returns>المسار النسبي للملف المحفوظ.</returns>
        public async Task<string> SaveFileAsync(IFormFile file, string subfolder)
        {
            var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", subfolder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("uploads", subfolder, fileName).Replace('\\', '/');
        }

        /// <summary>
        /// يحذف ملفًا ماديًا من الخادم بناءً على مساره النسبي.
        /// </summary>
        /// <param name="relativePath">المسار النسبي للملف (e.g., 'uploads/folder/file.jpg').</param>
        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            var fullPath = Path.Combine(_hostEnvironment.WebRootPath, relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        /// <summary>
        /// يسترجع بيانات ملف مادي (بايتات، نوع المحتوى، اسم الملف) من الخادم.
        /// </summary>
        /// <param name="relativePath">المسار النسبي للملف.</param>
        /// <returns>Tuple يحتوي على بيانات الملف أو قيم null إذا لم يتم العثور عليه.</returns>
        public (byte[] fileBytes, string contentType, string fileName) GetPhysicalFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return (null, null, null);
            }

            var fullPath = Path.Combine(_hostEnvironment.WebRootPath, relativePath);

            if (!File.Exists(fullPath))
            {
                return (null, null, null);
            }

            var fileBytes = File.ReadAllBytes(fullPath);
            var contentType = GetMimeType(fullPath);
            var fileName = Path.GetFileName(fullPath);

            return (fileBytes, contentType, fileName);
        }

        #endregion

        #region دوال مساعدة

        /// <summary>
        /// يحدد نوع المحتوى (MIME type) لملف بناءً على امتداده.
        /// </summary>
        private string GetMimeType(string filePath)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream"; // نوع افتراضي للملفات غير المعروفة.
            }
            return contentType;
        }

        #endregion
    }
}