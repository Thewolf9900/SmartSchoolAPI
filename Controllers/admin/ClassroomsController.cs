using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Classroom;
using SmartSchoolAPI.DTOs.Lecture;
using SmartSchoolAPI.DTOs.Material;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SmartSchoolAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/classrooms")]
    [Authorize(Roles = "Administrator")]
    public class ClassroomsController : ControllerBase
    {
        private readonly IClassroomRepository _classroomRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IUserRepository _userRepo;
        private readonly ILectureRepository _lectureRepo;
        private readonly IArchiveService _archiveService;

        public ClassroomsController(IClassroomRepository classroomRepo, ICourseRepository courseRepo, IUserRepository userRepo,
         ILectureRepository lectureRepo, IArchiveService archiveService)
        {
            _classroomRepo = classroomRepo;
            _courseRepo = courseRepo;
            _userRepo = userRepo;
            _lectureRepo = lectureRepo;
            _archiveService = archiveService;
        }

        #region عمليات قراءة الفصول

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClassroomDto>>> GetClassrooms([FromQuery] ClassroomStatus? status)
        {
            var classrooms = await _classroomRepo.GetAllClassroomsAsync(status);
            var classroomsDto = classrooms.Select(c => new ClassroomDto
            {
                ClassroomId = c.ClassroomId,
                Name = c.Name,
                CourseId = c.CourseId,
                CourseName = c.Course.Name,
                AcademicProgramId = c.Course.AcademicProgram.AcademicProgramId,
                AcademicProgramName = c.Course.AcademicProgram.Name,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher != null ? $"{c.Teacher.FirstName} {c.Teacher.LastName}" : "غير معين",
                Capacity = c.Capacity,
                Status = c.Status.ToString(),
                EnrolledStudentsCount = c.Enrollments.Count()
            });
            return Ok(classroomsDto);
        }

        [HttpGet("{id}", Name = "GetAdminClassroomById")]  
        public async Task<ActionResult<ClassroomDto>> GetClassroomById(int id)
        {
            var classroom = await _classroomRepo.GetClassroomByIdAsync(id);
            if (classroom == null) return NotFound(new { message = $"لم يتم العثور على فصل بالمعرّف {id}." });

            var classroomDto = new ClassroomDto
            {
                ClassroomId = classroom.ClassroomId,
                Name = classroom.Name,
                CourseId = classroom.CourseId,
                CourseName = classroom.Course?.Name ?? "Unknown Course",
                AcademicProgramId = classroom.Course?.AcademicProgram?.AcademicProgramId ?? 0,
                AcademicProgramName = classroom.Course?.AcademicProgram?.Name ?? "Unknown Program",
                Status = classroom.Status.ToString(),
                Capacity = classroom.Capacity,
                TeacherId = classroom.TeacherId,
                TeacherName = classroom.Teacher != null ? $"{classroom.Teacher.FirstName} {classroom.Teacher.LastName}" : "غير معين"
            };
            return Ok(classroomDto);
        }

        [HttpGet("{classroomId}/content")]
        public async Task<ActionResult<IEnumerable<LectureContentDto>>> GetClassroomContent(int classroomId)
        {
            var classroom = await _classroomRepo.GetClassroomByIdAsync(classroomId);
            if (classroom == null) return NotFound(new { message = "لم يتم العثور على الفصل." });

            var lectures = await _lectureRepo.GetLecturesWithMaterialsAsync(classroomId);
            var lecturesDto = lectures.Select(l => new LectureContentDto
            {
                LectureId = l.LectureId,
                Title = l.Title,
                Description = l.Description,
                LectureOrder = l.LectureOrder,
                CreatedAt = l.CreatedAt,
                Materials = l.Materials.Select(m => new MaterialDto
                {
                    MaterialId = m.MaterialId,
                    Title = m.Title,
                    Description = m.Description,
                    MaterialType = m.MaterialType,
                    OriginalFilename = m.OriginalFilename,
                    FileSize = m.FileSize,
                    UploadedAt = m.UploadedAt
                }).ToList()
            }).ToList();
            return Ok(lecturesDto);
        }

        #endregion

        #region عمليات إنشاء وتعديل وحذف الفصول

        [HttpPost]
        public async Task<ActionResult<ClassroomDto>> CreateClassroom(CreateClassroomDto createDto)
        {
            var course = await _courseRepo.GetCourseByIdAsync(createDto.CourseId);
            if (course == null) return BadRequest(new { message = "الدورة المحددة غير موجودة." });

            var classroomEntity = new Classroom
            {
                Name = createDto.Name,
                CourseId = createDto.CourseId,
                Capacity = createDto.Capacity ?? 20,  
                Status = ClassroomStatus.ACTIVE,
                CreatedAt = DateTime.UtcNow
            };

            await _classroomRepo.CreateClassroomAsync(classroomEntity);
            await _classroomRepo.SaveChangesAsync();

            var classroomDto = new ClassroomDto
            {
                ClassroomId = classroomEntity.ClassroomId,
                Name = classroomEntity.Name,
                Status = classroomEntity.Status.ToString(),
                Capacity = classroomEntity.Capacity,
                CourseId = course.CourseId,
                CourseName = course.Name,
                AcademicProgramId = course.AcademicProgramId,
                AcademicProgramName = course.AcademicProgram?.Name ?? "Unknown Program",
                TeacherName = "غير معين"
            };
            return CreatedAtAction(nameof(GetClassroomById), new { id = classroomEntity.ClassroomId }, classroomDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClassroom(int id, UpdateClassroomDto updateDto)
        {
            var classroomFromRepo = await _classroomRepo.GetClassroomByIdAsync(id);
            if (classroomFromRepo == null) return NotFound(new { message = "لم يتم العثور على الفصل." });
            if (classroomFromRepo.Status != ClassroomStatus.ACTIVE) return BadRequest(new { message = "لا يمكن تحديث فصل غير نشط." });

            var courseExists = await _courseRepo.GetCourseByIdAsync(updateDto.CourseId);
            if (courseExists == null) return BadRequest(new { message = "الدورة المحددة غير موجودة." });

            classroomFromRepo.Name = updateDto.Name;
            classroomFromRepo.CourseId = updateDto.CourseId;
            classroomFromRepo.Capacity = updateDto.Capacity;

            await _classroomRepo.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClassroom(int id)
        {
            var classroomFromRepo = await _classroomRepo.GetClassroomByIdAsync(id);
            if (classroomFromRepo == null) return NotFound(new { message = "لم يتم العثور على الفصل." });
            if (classroomFromRepo.Status != ClassroomStatus.ACTIVE) return BadRequest(new { message = "لا يمكن حذف إلا الفصول النشطة. يرجى أرشفة الفصول المكتملة." });
            if (classroomFromRepo.Enrollments.Any()) return BadRequest(new { message = "لا يمكن حذف فصل يحتوي على طلاب مسجلين." });

            _classroomRepo.DeleteClassroom(classroomFromRepo);
            await _classroomRepo.SaveChangesAsync();
            return NoContent();
        }

        #endregion

        #region عمليات إدارة المدرسين والأرشفة

        [HttpPost("{id}/assign-teacher")]
        public async Task<IActionResult> AssignTeacher(int id, AssignTeacherDto assignDto)
        {
            var classroom = await _classroomRepo.GetClassroomByIdAsync(id);
            if (classroom == null) return NotFound(new { message = "لم يتم العثور على الفصل." });
            if (classroom.Status != ClassroomStatus.ACTIVE) return BadRequest(new { message = "لا يمكن تعيين مدرس لفصل غير نشط." });

            var teacher = await _userRepo.GetUserByIdAsync(assignDto.TeacherId);
            if (teacher == null || teacher.Role != UserRole.Teacher) return BadRequest(new { message = "المستخدم المحدد ليس مدرسًا صالحًا." });

            classroom.TeacherId = assignDto.TeacherId;
            await _classroomRepo.SaveChangesAsync();
            return Ok(new { message = "تم تعيين المدرس بنجاح." });
        }

        [HttpDelete("{classroomId}/teacher")]
        public async Task<IActionResult> UnassignTeacher(int classroomId)
        {
            var classroom = await _classroomRepo.GetClassroomByIdAsync(classroomId);
            if (classroom == null) return NotFound(new { message = "لم يتم العثور على الفصل." });
            if (classroom.Status != ClassroomStatus.ACTIVE) return BadRequest(new { message = "لا يمكن إلغاء تعيين مدرس من فصل غير نشط." });
            if (classroom.TeacherId == null) return BadRequest(new { message = "هذا الفصل ليس لديه مدرس معين بالفعل." });

            classroom.TeacherId = null;
            await _classroomRepo.SaveChangesAsync();
            return Ok(new { message = "تم إلغاء تعيين المدرس بنجاح." });
        }

        [HttpPost("{classroomId}/archive")]
        public async Task<IActionResult> ArchiveClassroom(int classroomId)
        {
            var (success, message) = await _archiveService.ArchiveClassroomAsync(classroomId);
            if (success) return Ok(new { message });
            return BadRequest(new { message });
        }

        #endregion

        #region نقاط نهاية مساعدة (غير تابعة للمورد الرئيسي)

        [HttpGet("~/api/admin/courses/{courseId}/classrooms")]
        public async Task<ActionResult<IEnumerable<ClassroomDto>>> GetClassroomsForCourse(int courseId)
        {
            var course = await _courseRepo.GetCourseByIdAsync(courseId);
            if (course == null) return NotFound(new { message = "لم يتم العثور على الدورة." });
            var classrooms = await _classroomRepo.GetClassroomsByCourseAsync(courseId);
            var classroomsDto = classrooms.Select(c => new ClassroomDto
            {
                ClassroomId = c.ClassroomId,
                Name = c.Name,
                CourseId = c.CourseId,
                CourseName = c.Course.Name,
                AcademicProgramId = c.Course.AcademicProgram.AcademicProgramId,
                AcademicProgramName = c.Course.AcademicProgram.Name,
                TeacherId = c.TeacherId,
                Status = c.Status.ToString(),
                Capacity = c.Capacity,
                TeacherName = c.Teacher != null ? $"{c.Teacher.FirstName} {c.Teacher.LastName}" : "غير معين"
            });
            return Ok(classroomsDto);
        }

        [HttpGet("~/api/admin/teachers/{teacherId}/classrooms")]
        public async Task<ActionResult<IEnumerable<ClassroomDto>>> GetClassroomsForTeacher(int teacherId)
        {
            var user = await _userRepo.GetUserByIdAsync(teacherId);
            if (user == null || user.Role != UserRole.Teacher) return NotFound(new { message = "لم يتم العثور على المدرس." });
            var classrooms = await _classroomRepo.GetClassroomsByTeacherAsync(teacherId);
            var classroomsDto = classrooms.Select(c => new ClassroomDto
            {
                ClassroomId = c.ClassroomId,
                Name = c.Name,
                CourseId = c.CourseId,
                CourseName = c.Course.Name,
                AcademicProgramId = c.Course.AcademicProgram.AcademicProgramId,
                AcademicProgramName = c.Course.AcademicProgram.Name,
                TeacherId = c.TeacherId,
                Status = c.Status.ToString(),
                Capacity = c.Capacity,
                TeacherName = $"{user.FirstName} {user.LastName}"
            });
            return Ok(classroomsDto);
        }

        #endregion
    }
}