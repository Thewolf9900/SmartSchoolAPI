using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using SmartSchoolAPI.DTOs; // تمت إضافة هذا
using SmartSchoolAPI.Interfaces;
using SmartSchoolAPI.Settings;
using System.Text;
using UglyToad.PdfPig;

namespace SmartSchoolAPI.Services
{
    /// <summary>
    /// خدمة مسؤولة عن كافة عمليات التعامل مع الملفات،
    /// بما في ذلك الرفع إلى Cloudinary، الحذف، واستخلاص المحتوى النصي بشكل متقدم.
    /// </summary>
    public class FileService : IFileService
    {
        private readonly Cloudinary _cloudinary;

        public FileService(IOptions<CloudinarySettings> config)
        {
            if (config.Value == null ||
                string.IsNullOrEmpty(config.Value.CloudName) ||
                string.IsNullOrEmpty(config.Value.ApiKey) ||
                string.IsNullOrEmpty(config.Value.ApiSecret))
            {
                throw new ArgumentException("إعدادات Cloudinary غير مكتملة أو مفقودة.");
            }

            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }

        #region استخلاص المحتوى للذكاء الاصطناعي
         public async Task<string> ReadFileContentAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (fileExtension != ".pdf")
            {
                using var reader = new StreamReader(file.OpenReadStream());
                return await reader.ReadToEndAsync();
            }

            const int numberOfPagesToSample = 5;
            const int maxCharacterLimit = 2500;
            var allPagesText = new List<string>();

            using (var stream = file.OpenReadStream())
            {
                using PdfDocument document = PdfDocument.Open(stream);
                if (document.NumberOfPages == 0) return string.Empty;

                foreach (UglyToad.PdfPig.Content.Page page in document.GetPages())
                {
                    if (!string.IsNullOrWhiteSpace(page.Text))
                    {
                        allPagesText.Add(page.Text);
                    }
                }
            }

            var random = new Random();
            int pagesToTake = Math.Min(numberOfPagesToSample, allPagesText.Count);
            var sampledPages = allPagesText.OrderBy(x => random.Next()).Take(pagesToTake).ToList();
            var textBuilder = new StringBuilder();

            foreach (var pageText in sampledPages)
            {
                if (textBuilder.Length + pageText.Length > maxCharacterLimit)
                {
                    int remainingChars = maxCharacterLimit - textBuilder.Length;
                    if (remainingChars > 0)
                    {
                        textBuilder.Append(pageText.Substring(0, Math.Min(pageText.Length, remainingChars)));
                    }
                    break;
                }
                textBuilder.AppendLine(pageText);
            }

            if (textBuilder.Length > maxCharacterLimit)
            {
                textBuilder.Length = maxCharacterLimit;
            }

            return textBuilder.ToString();
        }
        #endregion

        #region عمليات نظام الملفات (تم التعديل هنا)

        /// <summary>
        /// يرفع ملفًا إلى خدمة التخزين السحابي ويعيد تفاصيله.
        /// </summary>
        /// <param name="file">الملف المراد رفعه.</param>
        /// <param name="subfolder">المجلد الفرعي داخل Cloudinary لتنظيم الملفات.</param>
        /// <returns>مهمة تمثل كائنًا يحتوي على رابط الملف العام (Url) والمعرف الفريد (PublicId).</returns>
        public async Task<FileUploadResult> SaveFileAsync(IFormFile file, string subfolder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("الملف فارغ أو غير موجود.", nameof(file));
            }

            await using var stream = file.OpenReadStream();
            var folderPath = !string.IsNullOrWhiteSpace(subfolder) ? $"smart-school/{subfolder}" : "smart-school/general";

            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (IsImageFile(fileExtension) || fileExtension == ".pdf")
            {
                return await UploadImageAsync(file.FileName, stream, folderPath);
            }
            else if (IsVideoFile(fileExtension))
            {
                return await UploadVideoAsync(file.FileName, stream, folderPath);
            }
            else
            {
                throw new ArgumentException("نوع الملف غير مدعوم");
            }
        }

        private async Task<FileUploadResult> UploadImageAsync(string fileName, Stream stream, string folderPath)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = folderPath,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadLargeAsync(uploadParams);
            return CreateResult(uploadResult);
        }

        private async Task<FileUploadResult> UploadVideoAsync(string fileName, Stream stream, string folderPath)
        {
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = folderPath,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadLargeAsync(uploadParams);
            return CreateResult(uploadResult);
        }



        private FileUploadResult CreateResult(UploadResult uploadResult)
        {
            if (uploadResult.Error != null)
            {
                throw new Exception($"فشل رفع الملف: {uploadResult.Error.Message}");
            }

            return new FileUploadResult
            {
                Url = uploadResult.SecureUrl.ToString(),
                PublicId = uploadResult.PublicId
            };
        }

        private bool IsImageFile(string extension)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff" };
            return imageExtensions.Contains(extension);
        }

        private bool IsVideoFile(string extension)
        {
            var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mkv" };
            return videoExtensions.Contains(extension);
        }

        /// <summary>
        /// يحذف ملفًا من خدمة التخزين السحابي باستخدام المعرف الفريد الخاص به.
        /// </summary>
        /// <param name="publicId">المعرف الفريد (PublicId) للملف في Cloudinary.</param>
        public async Task DeleteFileAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId)) return;

            var deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Raw };
            await _cloudinary.DestroyAsync(deletionParams);
        }

        
        #endregion
    }
}