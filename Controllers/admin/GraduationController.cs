using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Graduation;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http; // تمت إضافته
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/graduation")]
    [Authorize(Roles = "Administrator")]
    public class GraduationController : ControllerBase
    {
        private readonly IGraduationRepository _graduationRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IAcademicProgramRepository _programRepo;
        private readonly IFileService _fileService; // تمت إضافته
        private readonly IHttpClientFactory _httpClientFactory; // تمت إضافته

        public GraduationController(IGraduationRepository graduationRepo, IUserRepository userRepo,
            ICourseRepository courseRepo, IAcademicProgramRepository programRepo, IFileService fileService, IHttpClientFactory httpClientFactory) // تم تعديله
        {
            _graduationRepo = graduationRepo;
            _userRepo = userRepo;
            _courseRepo = courseRepo;
            _programRepo = programRepo;
            _fileService = fileService; // تمت إضافته
            _httpClientFactory = httpClientFactory; // تمت إضافته
        }

        #region عملية التخرج والرسوب

        [HttpPost("process/program/{programId}")]
        public async Task<IActionResult> ProcessProgramGraduations(int programId)
        {
            var program = await _programRepo.GetProgramByIdAsync(programId);
            if (program == null) return NotFound(new { message = "لم يتم العثور على البرنامج." });

            var programCourses = (await _courseRepo.GetCoursesByProgramAsync(programId)).ToList();
            if (!programCourses.Any()) return Ok(new { message = "البرنامج لا يحتوي على دورات ليتم تقييمها." });

            var programCourseIds = programCourses.Select(c => c.CourseId).ToHashSet();
            var studentsInProgram = await _userRepo.GetStudentsWithEnrollmentsByProgramAsync(programId);

            var newGraduates = new List<Graduation>();
            var newFailures = new List<FailedStudent>();

            foreach (var student in studentsInProgram)
            {
                bool alreadyProcessed = await _graduationRepo.HasAlreadyGraduatedAsync(student.UserId, programId) ||
                                        await _graduationRepo.HasFailedAsync(student.UserId, programId);
                if (alreadyProcessed) continue;

                var relevantEnrollments = student.Enrollments
                    .Where(e => e.Classroom.Status == Enums.ClassroomStatus.COMPLETED && e.FinalGrade.HasValue && programCourseIds.Contains(e.Classroom.CourseId))
                    .ToList();

                var completedCourseIds = relevantEnrollments.Select(e => e.Classroom.CourseId).ToHashSet();

                if (programCourseIds.IsSubsetOf(completedCourseIds) && relevantEnrollments.Any())
                {
                    var gpa = relevantEnrollments.Average(e => e.FinalGrade.Value);

                    if (gpa >= 60) // ناجح
                    {
                        newGraduates.Add(new Graduation
                        {
                            StudentUserId = student.UserId,
                            FirstName = student.FirstName,
                            LastName = student.LastName,
                            Email = student.Email,
                            NationalId = student.NationalId,
                            AcademicProgramId = programId,
                            ProgramNameAtGraduation = program.Name,
                            GraduationDate = DateTime.UtcNow,
                            FinalGpa = gpa
                        });
                    }
                    else // راسب
                    {
                        newFailures.Add(new FailedStudent
                        {
                            StudentUserId = student.UserId,
                            FirstName = student.FirstName,
                            LastName = student.LastName,
                            Email = student.Email,
                            NationalId = student.NationalId,
                            AcademicProgramId = programId,
                            ProgramNameAtFailure = program.Name,
                            FailureDate = DateTime.UtcNow,
                            FinalGpa = gpa
                        });
                    }
                }
            }

            if (!newGraduates.Any() && !newFailures.Any())
            {
                return Ok(new { message = "لم يكن هناك طلاب جدد مؤهلون للمعالجة." });
            }

            foreach (var gradRecord in newGraduates)
            {
                await _graduationRepo.CreateGraduationRecordAsync(gradRecord);
            }
            foreach (var failRecord in newFailures)
            {
                await _graduationRepo.CreateFailedStudentRecordAsync(failRecord);
            }

            await _graduationRepo.SaveChangesAsync();

            return Ok(new { message = $"اكتملت المعالجة. عدد الخريجين الجدد: {newGraduates.Count}، عدد الراسبين الجدد: {newFailures.Count}." });
        }

        #endregion

        #region عرض سجلات الخريجين والراسبين

        [HttpGet("graduates")]
        public async Task<ActionResult<IEnumerable<GraduateDto>>> GetGraduates([FromQuery] int? programId, [FromQuery] int? year, [FromQuery] int? month)
        {
            if (month.HasValue && !year.HasValue) return BadRequest(new { message = "لا يمكن استخدام فلتر الشهر بدون تحديد السنة." });
            var graduates = await _graduationRepo.GetGraduatesAsync(programId, year, month);
            var dtos = graduates.Select(g => new GraduateDto
            {
                GraduationId = g.GraduationId,
                StudentUserId = g.StudentUserId,
                FirstName = g.FirstName,
                LastName = g.LastName,
                NationalId = g.NationalId,
                ProgramName = g.ProgramNameAtGraduation,
                GraduationDate = g.GraduationDate,
                FinalGpa = g.FinalGpa,
                HasCertificate = g.Certificate != null
            });
            return Ok(dtos);
        }

        [HttpGet("failures")]
        public async Task<ActionResult<IEnumerable<FailedStudentDto>>> GetFailures([FromQuery] int? programId, [FromQuery] int? year, [FromQuery] int? month)
        {
            if (month.HasValue && !year.HasValue) return BadRequest(new { message = "لا يمكن استخدام فلتر الشهر بدون تحديد السنة." });
            var failures = await _graduationRepo.GetFailuresAsync(programId, year, month);
            var dtos = failures.Select(f => new FailedStudentDto
            {
                FailureId = f.FailureId,
                StudentUserId = f.StudentUserId,
                FirstName = f.FirstName,
                LastName = f.LastName,
                NationalId = f.NationalId,
                ProgramName = f.ProgramNameAtFailure,
                FailureDate = f.FailureDate,
                FinalGpa = f.FinalGpa,
                Notes = f.Notes
            });
            return Ok(dtos);
        }

        #endregion

        #region إدارة الشهادات

        [HttpGet("{graduationId}/certificate")]
        public async Task<IActionResult> GetCertificate(int graduationId)
        {
            var graduation = await _graduationRepo.GetGraduationByIdAsync(graduationId);
            if (graduation?.Certificate == null || string.IsNullOrWhiteSpace(graduation.Certificate.CertificateUrl))
                return NotFound(new { message = "لم يتم العثور على الشهادة." });

            try
            {
                var client = _httpClientFactory.CreateClient();
                var fileBytes = await client.GetByteArrayAsync(graduation.Certificate.CertificateUrl);
                return File(fileBytes, graduation.Certificate.FileType, graduation.Certificate.FileName);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "فشل تحميل ملف الشهادة من الخدمة السحابية." });
            }
        }

        [HttpPost("{graduationId}/upload-certificate")]
        public async Task<IActionResult> UploadCertificate(int graduationId, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "لم يتم رفع أي ملف." });
            if (file.Length > 5 * 1024 * 1024) return BadRequest(new { message = "يجب أن يكون حجم الملف أقل من 5 ميجابايت." });

            var graduation = await _graduationRepo.GetGraduationByIdAsync(graduationId);
            if (graduation == null) return NotFound(new { message = "لم يتم العثور على سجل التخرج." });
            if (graduation.Certificate != null) return BadRequest(new { message = "تم رفع شهادة بالفعل. يرجى حذف القديمة أولاً." });

            var uploadResult = await _fileService.SaveFileAsync(file, "certificates");

            var certificate = new GraduationCertificate
            {
                GraduationId = graduationId,
                FileName = file.FileName,
                FileType = file.ContentType,
                CertificateUrl = uploadResult.Url,
                PublicId = uploadResult.PublicId
            };

            await _graduationRepo.AddCertificateAsync(certificate);
            await _graduationRepo.SaveChangesAsync();

            return Ok(new { message = "تم رفع الشهادة بنجاح." });
        }

        [HttpDelete("{graduationId}/certificate")]
        public async Task<IActionResult> DeleteCertificate(int graduationId)
        {
            var graduation = await _graduationRepo.GetGraduationByIdAsync(graduationId);
            if (graduation?.Certificate == null || string.IsNullOrWhiteSpace(graduation.Certificate.PublicId))
                return NotFound(new { message = "لم يتم العثور على الشهادة." });

            await _fileService.DeleteFileAsync(graduation.Certificate.PublicId);

            await _graduationRepo.DeleteCertificateAsync(graduationId);
            await _graduationRepo.SaveChangesAsync();

            return NoContent();
        }

        #endregion
    }
}