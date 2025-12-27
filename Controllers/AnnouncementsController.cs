using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Announcement;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers
{
    [ApiController]
    [Route("api/announcements")]
    [Authorize]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementRepository _announcementRepo;
        private readonly IAcademicProgramRepository _programRepo;
        private readonly IClassroomRepository _classroomRepo;
        private readonly ICourseRepository _courseRepo;

        public AnnouncementsController(IAnnouncementRepository announcementRepo, IAcademicProgramRepository programRepo,
            IClassroomRepository classroomRepo, ICourseRepository courseRepo)
        {
            _announcementRepo = announcementRepo;
            _programRepo = programRepo;
            _classroomRepo = classroomRepo;
            _courseRepo = courseRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnnouncementDto>>> GetAnnouncements()
        {
             var announcements = await _announcementRepo.GetAllAnnouncementsAsync();
            var announcementsDto = new List<AnnouncementDto>();

            foreach (var a in announcements)
            {
                var dto = new AnnouncementDto
                {
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Content = a.Content,
                    PostedAt = a.PostedAt,
                    TargetScope = a.TargetScope
                };

                switch (a.TargetScope)
                {
                    case AnnouncementScope.PROGRAM:
                        dto.TargetId = a.AcademicProgramId;
                        dto.TargetName = a.AcademicProgram?.Name;
                        break;
                    case AnnouncementScope.COURSE:
                        dto.TargetId = a.CourseId;
                        dto.TargetName = a.Course?.Name;
                        break;
                    case AnnouncementScope.CLASSROOM:
                        dto.TargetId = a.ClassroomId;
                        dto.TargetName = a.Classroom != null ? $"{a.Classroom.Name} ({a.Classroom.Course?.Name})" : null;
                        break;
                    case AnnouncementScope.GLOBAL:
                        dto.TargetId = null;
                        dto.TargetName = "All School";
                        break;
                }
                announcementsDto.Add(dto);
            }
            return Ok(announcementsDto);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Teacher")]
        public async Task<IActionResult> CreateAnnouncement(CreateAnnouncementDto createDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var announcementEntity = new Announcement
            {
                Title = createDto.Title,
                Content = createDto.Content,
                TargetScope = createDto.TargetScope,
                CreatedByUserId = userId
            };

            if (userRole == "Teacher")
            {
                if (createDto.TargetScope != AnnouncementScope.CLASSROOM || !createDto.TargetId.HasValue)
                    return BadRequest(new { message = "يمكن للمعلمين فقط إنشاء إعلانات على مستوى الفصل الدراسي ويجب عليهم توفير معرف الفصل الدراسي." });

                var classroom = await _classroomRepo.GetClassroomByIdAsync(createDto.TargetId.Value);
                if (classroom == null)
                    return NotFound(new { message = "الفصل الدراسي المحدد غير موجود." });
                if (classroom.TeacherId != userId)
                    return Forbid(); 

                announcementEntity.ClassroomId = createDto.TargetId.Value;
            }
            else // Administrator
            {
                switch (createDto.TargetScope)
                {
                    case AnnouncementScope.PROGRAM:
                        if (!createDto.TargetId.HasValue || await _programRepo.GetProgramByIdAsync(createDto.TargetId.Value) == null)
                            return BadRequest(new { message = "معرف البرنامج التعليمي غير صالج " });
                        announcementEntity.AcademicProgramId = createDto.TargetId.Value;
                        break;
                    case AnnouncementScope.COURSE:
                        if (!createDto.TargetId.HasValue || await _courseRepo.GetCourseByIdAsync(createDto.TargetId.Value) == null)
                            return BadRequest(new { message = "معرف الدورة غير صالح " });
                        announcementEntity.CourseId = createDto.TargetId.Value;
                        break;
                    case AnnouncementScope.CLASSROOM:
                        if (!createDto.TargetId.HasValue || await _classroomRepo.GetClassroomByIdAsync(createDto.TargetId.Value) == null)
                            return BadRequest(new { message = "معرف الصف غير صالح " });
                        announcementEntity.ClassroomId = createDto.TargetId.Value;
                        break;
                    case AnnouncementScope.GLOBAL:
                        break;
                }
            }

            await _announcementRepo.CreateAnnouncementAsync(announcementEntity);
            await _announcementRepo.SaveChangesAsync();
            return Ok(new { message = "تم انشاء اعلان بنجاح" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator, Teacher")]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var announcement = await _announcementRepo.GetAnnouncementByIdAsync(id);
            if (announcement == null) return NotFound(new { message = "لم يتم العثور على الاعلان " });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == "Teacher" && announcement.CreatedByUserId != userId)
            {
                return Forbid(); 
            }

            _announcementRepo.DeleteAnnouncement(announcement);
            await _announcementRepo.SaveChangesAsync();
            return NoContent();
        }
    }
}