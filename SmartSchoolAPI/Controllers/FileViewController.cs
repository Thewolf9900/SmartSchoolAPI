using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.Interfaces;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers
{
    [ApiController]
    [Route("api/view")]
    [Authorize(Roles = "Student,Teacher")] // متاح للطلاب والمدرسين المسجلين فقط
    public class FileViewController : ControllerBase
    {
        private readonly IMaterialRepository _materialRepo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IClassroomRepository _classroomRepo;
        private readonly IEnrollmentRepository _enrollmentRepo;

        public FileViewController(
            IMaterialRepository materialRepo,
            IHttpClientFactory httpClientFactory,
            IClassroomRepository classroomRepo,
            IEnrollmentRepository enrollmentRepo)
        {
            _materialRepo = materialRepo;
            _httpClientFactory = httpClientFactory;
            _classroomRepo = classroomRepo;
            _enrollmentRepo = enrollmentRepo;
        }

        [HttpGet("materials/{materialId}")]
        public async Task<IActionResult> ViewMaterial(int materialId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var material = await _materialRepo.GetMaterialWithDeepDetailsAsync(materialId);

            if (material == null || material.MaterialType != "File" || string.IsNullOrEmpty(material.Url))
            {
                return NotFound("الملف غير موجود أو غير صالح للعرض.");
            }

            // التحقق من صلاحيات الوصول
            bool hasAccess = await CheckUserAccessAsync(userId.Value, material);
            if (!hasAccess)
            {
                return Forbid();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(material.Url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();

                // تحديد نوع المحتوى بناءً على امتداد الملف أو النوع المخزن
                var contentType = material.FileType ?? "application/octet-stream";

                // تمكين الـ seeking للفيديوهات
                return new FileStreamResult(stream, contentType)
                {
                    EnableRangeProcessing = true
                };
            }
            catch (Exception ex)
            {
                // يمكنك تسجيل الخطأ هنا للمراقبة
                // LogError(ex);
                return StatusCode(500, "حدث خطأ أثناء محاولة جلب الملف.");
            }
        }

        private async Task<bool> CheckUserAccessAsync(int userId, Entities.Material material)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == "Student")
            {
                if (material.Lecture?.Classroom != null)
                {
                    // الطالب يجب أن يكون مسجلاً في الفصل
                    return material.Lecture.Classroom.Enrollments.Any(e => e.StudentId == userId);
                }
                if (material.Course != null)
                {
                    // الطالب يجب أن يكون مسجلاً في أي فصل ضمن هذه الدورة
                    return await _enrollmentRepo.IsStudentEnrolledInCourseAsync(userId, material.Course.CourseId);
                }
            }
            else if (userRole == "Teacher")
            {
                if (material.Lecture?.Classroom != null)
                {
                    // المدرس يجب أن يكون مدرس الفصل
                    return material.Lecture.Classroom.TeacherId == userId;
                }
                if (material.Course != null)
                {
                    // المدرس يجب أن يكون منسق الدورة أو يدرس في أي فصل ضمنها
                    if (material.Course.CoordinatorId == userId) return true;
                    return await _classroomRepo.IsTeacherAssociatedWithCourseAsync(userId, material.Course.CourseId);
                }
            }

            return false;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}