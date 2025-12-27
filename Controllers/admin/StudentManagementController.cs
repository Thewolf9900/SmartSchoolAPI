using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Enrollment;
using SmartSchoolAPI.DTOs.Profile;
using SmartSchoolAPI.DTOs.Reports;
using SmartSchoolAPI.DTOs.User;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/student-management")]
    [Authorize(Roles = "Administrator")]
    public class StudentManagementController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IAcademicProgramRepository _programRepo;
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly ICourseRepository _courseRepo;

        public StudentManagementController(IUserRepository userRepo, IAcademicProgramRepository programRepo,
            IEnrollmentRepository enrollmentRepo, ICourseRepository courseRepo)
        {
            _userRepo = userRepo;
            _programRepo = programRepo;
            _enrollmentRepo = enrollmentRepo;
            _courseRepo = courseRepo;
        }

        #region عرض قوائم الطلاب

        [HttpGet("active-students")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetActiveStudents()
        {
            var activeStudents = await _userRepo.GetActiveStudentsAsync();
            var studentsDto = activeStudents.Select(MapToUserDto);
            return Ok(studentsDto);
        }

        [HttpGet("unassigned-students")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUnassignedStudents()
        {
            var students = await _userRepo.GetUnassignedStudentsAsync();
            var studentsDto = students.Select(MapToUserDto);
            return Ok(studentsDto);
        }

        #endregion

        #region إدارة تعيين البرامج

        [HttpPost("{studentId}/assign-program")]
        public async Task<IActionResult> AssignProgramToStudent(int studentId, [FromBody] AssignProgramDto assignDto)
        {
            var student = await _userRepo.GetUserByIdAsync(studentId);
            if (student == null || student.Role != UserRole.Student)
                return NotFound(new { message = "لم يتم العثور على الطالب." });

            var program = await _programRepo.GetProgramByIdAsync(assignDto.AcademicProgramId);
            if (program == null)
                return BadRequest(new { message = "البرنامج الأكاديمي المحدد غير صالح." });

            student.AcademicProgramId = assignDto.AcademicProgramId;
            await _userRepo.SaveChangesAsync();
            return Ok(new { message = $"تم تعيين الطالب '{student.FirstName} {student.LastName}' بنجاح إلى برنامج '{program.Name}'." });
        }

        [HttpDelete("{studentId}/unassign-program")]
        public async Task<IActionResult> UnassignProgramFromStudent(int studentId)
        {
            var student = await _userRepo.GetUserByIdAsync(studentId);
            if (student == null || student.Role != UserRole.Student)
                return NotFound(new { message = "لم يتم العثور على الطالب." });

            if (student.AcademicProgramId == null)
                return BadRequest(new { message = "هذا الطالب غير معين لأي برنامج." });

            var studentEnrollments = await _enrollmentRepo.GetEnrollmentsForStudentAsync(studentId);
            foreach (var enrollment in studentEnrollments)
            {
                _enrollmentRepo.DeleteEnrollment(enrollment);
            }

            student.AcademicProgramId = null;
            await _userRepo.SaveChangesAsync();

            return Ok(new { message = "تم إلغاء تعيين الطالب من البرنامج بنجاح، وتم حذف جميع تسجيلاته." });
        }

        #endregion

        #region عرض الملف الشخصي والبيانات المساعدة

        [HttpGet("{studentId}/profile")]
        public async Task<ActionResult<StudentProfileDto>> GetStudentProfile(int studentId)
        {
            var student = await _userRepo.GetStudentProfileByIdAsync(studentId);
            if (student == null)
                return NotFound(new { message = "لم يتم العثور على الطالب." });

            if (student.AcademicProgramId == null)
                return BadRequest(new { message = "هذا الطالب غير نشط ولا يملك ملفًا شخصيًا كاملاً. يرجى تعيينه لبرنامج أولاً." });

            var programCourses = (await _courseRepo.GetCoursesByProgramAsync(student.AcademicProgramId.Value)).ToList();
            var programCourseIds = programCourses.Select(c => c.CourseId).ToHashSet();
            var enrolledCourseIds = student.Enrollments.Select(e => e.Classroom.CourseId).ToHashSet();

            var missingCourses = programCourses
                .Where(c => !enrolledCourseIds.Contains(c.CourseId))
                .Select(c => new CourseShortDto { CourseId = c.CourseId, CourseName = c.Name })
                .ToList();

            var profileDto = new StudentProfileDto
            {
                UserInfo = MapToUserDto(student),
                ProgramName = student.AcademicProgram.Name,
                Enrollments = student.Enrollments.Select(e => new EnrollmentDto
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
                }).ToList(),
                MissingCourses = missingCourses
            };

            return Ok(profileDto);
        }

        [HttpGet("courses/{courseId}/available-students")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAvailableStudentsForCourse(int courseId)
        {
            var course = await _courseRepo.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return NotFound(new { message = "لم يتم العثور على الدورة." });
            }
            var students = await _userRepo.GetAvailableStudentsForCourseAsync(courseId);
            var studentsDto = students.Select(MapToUserDto);
            return Ok(studentsDto);
        }

        #endregion

        #region نقاط نهاية مساعدة (تتبع موارد أخرى)

        [HttpGet("~/api/admin/programs/{programId}/students")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetStudentsInProgram(int programId)
        {
            var program = await _programRepo.GetProgramByIdAsync(programId);
            if (program == null)
                return NotFound(new { message = "لم يتم العثور على البرنامج." });

            var students = await _userRepo.GetStudentsByProgramAsync(programId);
            var studentsDto = students.Select(MapToUserDto);
            return Ok(studentsDto);
        }

        #endregion

        #region دوال مساعدة خاصة

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                NationalId = user.NationalId,
                Role = user.Role
            };
        }

        #endregion
    }
}