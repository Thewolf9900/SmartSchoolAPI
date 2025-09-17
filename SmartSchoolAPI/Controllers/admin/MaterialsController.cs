using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Material;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
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

        public MaterialsController(IMaterialRepository materialRepo, ICourseRepository courseRepo, IFileService fileService)
        {
            _materialRepo = materialRepo;
            _courseRepo = courseRepo;
            _fileService = fileService;
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

            var (fileBytes, contentType, fileName) = _fileService.GetPhysicalFile(material.Url);
            if (fileBytes == null)
            {
                return NotFound(new { message = "تعذر العثور على الملف الفعلي على الخادم." });
            }

            var downloadName = material.OriginalFilename ?? fileName;
            return File(fileBytes, contentType, downloadName);
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
                materialEntity.Url = await _fileService.SaveFileAsync(createDto.File, "course-materials");
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

            if (material.MaterialType == "File" && !string.IsNullOrEmpty(material.Url))
            {
                _fileService.DeleteFile(material.Url);
            }

            _materialRepo.DeleteMaterial(material);
            await _materialRepo.SaveChangesAsync();
            return NoContent();
        }
    }
}