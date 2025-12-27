using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Enrollment;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/enrollments")]
    [Authorize(Roles = "Administrator")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly IUserRepository _userRepo;
        private readonly IClassroomRepository _classroomRepo;

        public EnrollmentsController(IEnrollmentRepository enrollmentRepo, IUserRepository userRepo, IClassroomRepository classroomRepo)
        {
            _enrollmentRepo = enrollmentRepo;
            _userRepo = userRepo;
            _classroomRepo = classroomRepo;
        }

        [HttpPost]
        public async Task<IActionResult> EnrollStudent([FromBody] CreateEnrollmentDto createDto)
        {
            var student = await _userRepo.GetUserByIdAsync(createDto.StudentId);
            if (student == null || student.Role != UserRole.Student)
            {
                return BadRequest(new { message = "معرّف الطالب غير صالح." });
            }

            if (student.AcademicProgramId == null)
            {
                return BadRequest(new { message = "يجب تعيين الطالب لبرنامج أكاديمي أولاً قبل التسجيل." });
            }

            var classroom = await _classroomRepo.GetClassroomByIdAsync(createDto.ClassroomId);
            if (classroom == null)
            {
                return BadRequest(new { message = "معرّف الفصل غير صالح." });
            }

            if (classroom.Status != ClassroomStatus.ACTIVE)
            {
                return BadRequest(new { message = "لا يمكن تسجيل الطالب في فصل غير نشط." });
            }

            if (student.AcademicProgramId != classroom.Course.AcademicProgramId)
            {
                return BadRequest(new { message = "لا يمكن للطالب التسجيل في فصل يتبع لبرنامج أكاديمي مختلف." });
            }

            if (classroom.Enrollments.Count >= classroom.Capacity)
            {
                return BadRequest(new { message = "لا يمكن تسجيل الطالب. سعة الفصل مكتملة." });
            }

            var studentEnrollments = await _enrollmentRepo.GetEnrollmentsForStudentAsync(createDto.StudentId);
            if (studentEnrollments.Any(e => e.Classroom.CourseId == classroom.CourseId))
            {
                return BadRequest(new { message = "الطالب مسجل بالفعل في فصل آخر لنفس الدورة." });
            }

            var enrollmentEntity = new Enrollment
            {
                StudentId = createDto.StudentId,
                ClassroomId = createDto.ClassroomId,
                EnrollmentDate = DateTime.UtcNow
            };

            await _enrollmentRepo.CreateEnrollmentAsync(enrollmentEntity);
            await _enrollmentRepo.SaveChangesAsync();

            return Ok(new { message = "تم تسجيل الطالب بنجاح." });
        }

        [HttpPut("{enrollmentId}/transfer")]
        public async Task<IActionResult> TransferStudent(int enrollmentId, [FromBody] TransferStudentDto transferDto)
        {
            var enrollment = await _enrollmentRepo.GetEnrollmentByIdAsync(enrollmentId);
            if (enrollment == null)
            {
                return NotFound(new { message = "لم يتم العثور على سجل التسجيل." });
            }

            var oldClassroom = await _classroomRepo.GetClassroomByIdAsync(enrollment.ClassroomId);
            var newClassroom = await _classroomRepo.GetClassroomByIdAsync(transferDto.NewClassroomId);

            if (newClassroom == null)
            {
                return BadRequest(new { message = "الفصل الجديد المحدد غير موجود." });
            }

            if (oldClassroom == null)
            {
                 return StatusCode(500, new { message = "خطأ في النظام: لا يمكن العثور على الفصل القديم المرتبط بالتسجيل." });
            }

            if (oldClassroom.ClassroomId == newClassroom.ClassroomId)
            {
                return BadRequest(new { message = "لا يمكن نقل الطالب إلى نفس الفصل." });
            }

            if (oldClassroom.Status != ClassroomStatus.ACTIVE || newClassroom.Status != ClassroomStatus.ACTIVE)
            {
                return BadRequest(new { message = "لا يمكن نقل الطالب من أو إلى فصل غير نشط." });
            }

            if (newClassroom.Enrollments.Count >= newClassroom.Capacity)
            {
                return BadRequest(new { message = "الفصل الجديد مكتمل السعة." });
            }

            var studentEnrollments = await _enrollmentRepo.GetEnrollmentsForStudentAsync(enrollment.StudentId);
            if (studentEnrollments.Any(e => e.ClassroomId != enrollmentId && e.Classroom.CourseId == newClassroom.Course.CourseId))
            {
                return BadRequest(new { message = "الطالب مسجل بالفعل في دورة الفصل الجديد عبر فصل آخر." });
            }

            enrollment.ClassroomId = transferDto.NewClassroomId;
            await _enrollmentRepo.SaveChangesAsync();

            return Ok(new { message = "تم نقل الطالب بنجاح." });
        }

        [HttpDelete("{enrollmentId}")]
        public async Task<IActionResult> UnenrollStudent(int enrollmentId)
        {
            var enrollment = await _enrollmentRepo.GetEnrollmentWithDetailsByIdAsync(enrollmentId);
            if (enrollment == null)
            {
                return NotFound(new { message = "لم يتم العثور على سجل التسجيل." });
            }

            if (enrollment.Classroom?.Status != ClassroomStatus.ACTIVE)
            {
                return BadRequest(new { message = "لا يمكن إلغاء تسجيل طالب من فصل غير نشط." });
            }

            _enrollmentRepo.DeleteEnrollment(enrollment);
            await _enrollmentRepo.SaveChangesAsync();

            return NoContent();
        }

        #region نقاط نهاية مساعدة (للقراءة فقط)

        [HttpGet("~/api/admin/classrooms/{classroomId}/enrollments")]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetClassroomEnrollments(int classroomId)
        {
            var enrollments = await _enrollmentRepo.GetEnrollmentsForClassroomAsync(classroomId);
            var enrollmentsDto = enrollments.Select(e => new EnrollmentDto
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                StudentName = $"{e.Student.FirstName} {e.Student.LastName}",
                ClassroomId = e.ClassroomId,
                ClassroomName = e.Classroom.Name,
                EnrollmentDate = e.EnrollmentDate,
                PracticalGrade = e.PracticalGrade,
                ExamGrade = e.ExamGrade,
                FinalGrade = e.FinalGrade
            });

            return Ok(enrollmentsDto);
        }

        [HttpGet("~/api/admin/students/{studentId}/enrollments")]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetStudentEnrollments(int studentId)
        {
            var student = await _userRepo.GetUserByIdAsync(studentId);
            if (student == null || student.Role != UserRole.Student)
            {
                return NotFound(new { message = "لم يتم العثور على الطالب." });
            }

            var enrollments = await _enrollmentRepo.GetEnrollmentsForStudentAsync(studentId);
            var enrollmentsDto = enrollments.Select(e => new EnrollmentDto
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                StudentName = $"{student.FirstName} {student.LastName}",
                ClassroomId = e.ClassroomId,
                ClassroomName = $"{e.Classroom.Name} ({e.Classroom.Course.Name})",
                EnrollmentDate = e.EnrollmentDate,
                PracticalGrade = e.PracticalGrade,
                ExamGrade = e.ExamGrade,
                FinalGrade = e.FinalGrade
            });

            return Ok(enrollmentsDto);
        }

        #endregion
    }
}