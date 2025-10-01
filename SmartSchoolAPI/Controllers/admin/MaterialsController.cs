using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Material;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http; // تمت إضافته
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/materials")]
    [Authorize(Roles = "Administrator")]
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialRepository _materialRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IFileService _fileService;
        private readonly IHttpClientFactory _httpClientFactory; // تمت إضافته

        public MaterialsController(IMaterialRepository materialRepo, ICourseRepository courseRepo, IFileService fileService, IHttpClientFactory httpClientFactory) // تم تعديله
        {
            _materialRepo = materialRepo;
            _courseRepo = courseRepo;
            _fileService = fileService;
            _httpClientFactory = httpClientFactory; // تمت إضافته
        }

        [HttpGet("from-course/{courseId}")]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetCourseMaterials(int courseId)
        {
            var course = await _courseRepo.GetCourseByIdAsync(courseId);
            if (course == null)
                return NotFound(new { message = "لم يتم العثور على الدورة." });

            var materials = await _materialRepo.GetMaterialsForCourseAsync(courseId);

            var materialsDto = materials.Select(m => new MaterialDto
            {
                MaterialId = m.MaterialId,
                Title = m.Title,
                Description = m.Description,
                MaterialType = m.MaterialType,
                OriginalFilename = m.OriginalFilename,
                FileSize = m.FileSize,
                UploadedAt = m.UploadedAt
            });

            return Ok(materialsDto);
        }

        [HttpGet("{materialId}/download")]
        public async Task<IActionResult> DownloadMaterial(int materialId)
        {
            var material = await _materialRepo.GetMaterialByIdAsync(materialId);
            if (material == null || material.MaterialType != "File" || string.IsNullOrEmpty(material.Url))
            {
                return NotFound(new { message = "الملف غير موجود أو غير صالح للتحميل." });
            }

            // المنطق الجديد: تحميل الملف من الرابط وإعادته للمستخدم
            try
            {
                var client = _httpClientFactory.CreateClient();
                var fileBytes = await client.GetByteArrayAsync(material.Url);
                var downloadName = material.OriginalFilename ?? Path.GetFileName(new System.Uri(material.Url).LocalPath);

                // تحديد نوع المحتوى بشكل افتراضي إذا لم يكن متوفراً
                var contentType = "application/octet-stream";
                if (downloadName.EndsWith(".pdf")) contentType = "application/pdf";
                if (downloadName.EndsWith(".png")) contentType = "image/png";
                if (downloadName.EndsWith(".jpg") || downloadName.EndsWith(".jpeg")) contentType = "image/jpeg";

                return File(fileBytes, contentType, downloadName);
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { message = "فشل تحميل الملف من الخدمة السحابية." });
            }
        }

        [HttpPost("for-course/{courseId}")]
        public async Task<IActionResult> AddCourseMaterial(int courseId, [FromForm] CreateMaterialDto createDto)
        {
            var course = await _courseRepo.GetCourseByIdAsync(courseId);
            if (course == null)
                return NotFound(new { message = "لم يتم العثور على الدورة." });

            if (createDto.File == null && string.IsNullOrWhiteSpace(createDto.Url))
                return BadRequest(new { message = "يجب توفير ملف أو رابط." });

            if (createDto.File != null && !string.IsNullOrWhiteSpace(createDto.Url))
                return BadRequest(new { message = "يرجى توفير ملف أو رابط، وليس كلاهما." });

            var materialEntity = new Material
            {
                Title = createDto.Title,
                Description = createDto.Description,
                CourseId = courseId,
                UploadedAt = System.DateTime.UtcNow
            };

            if (createDto.File != null)
            {
                materialEntity.MaterialType = "File";

                var uploadResult = await _fileService.SaveFileAsync(createDto.File, "course-materials");
                materialEntity.Url = uploadResult.Url;
                materialEntity.PublicId = uploadResult.PublicId;

                materialEntity.OriginalFilename = createDto.File.FileName;
                materialEntity.FileSize = createDto.File.Length;
            }
            else
            {
                materialEntity.MaterialType = "Link";
                materialEntity.Url = createDto.Url!;
            }

            await _materialRepo.CreateMaterialAsync(materialEntity);
            await _materialRepo.SaveChangesAsync();

            return Ok(new { message = "تمت إضافة المادة المرجعية بنجاح." });
        }

        [HttpPut("{materialId}")]
        public async Task<IActionResult> UpdateMaterial(int materialId, [FromBody] UpdateMaterialDto updateDto)
        {
            var material = await _materialRepo.GetMaterialByIdAsync(materialId);
            if (material == null) return NotFound(new { message = "لم يتم العثور على المادة." });

            if (material.LectureId != null)
            {
                return Forbid("يمكن للمديرين تعديل المواد المرجعية على مستوى الدورة فقط.");
            }

            material.Title = updateDto.Title;
            material.Description = updateDto.Description;

            await _materialRepo.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{materialId}")]
        public async Task<IActionResult> DeleteMaterial(int materialId)
        {
            var material = await _materialRepo.GetMaterialByIdAsync(materialId);
            if (material == null) return NotFound(new { message = "لم يتم العثور على المادة." });

            if (material.LectureId != null)
            {
                return Forbid("يمكن للمديرين حذف المواد المرجعية على مستوى الدورة فقط.");
            }

            if (material.MaterialType == "File" && !string.IsNullOrEmpty(material.PublicId))
            {
                await _fileService.DeleteFileAsync(material.PublicId);
            }

            _materialRepo.DeleteMaterial(material);
            await _materialRepo.SaveChangesAsync();
            return NoContent();
        }
    }
}