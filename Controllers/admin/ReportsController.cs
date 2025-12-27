using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Classroom;
using SmartSchoolAPI.DTOs.Course;
using SmartSchoolAPI.DTOs.Graduation;
using SmartSchoolAPI.DTOs.Reports;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/reports")]
    [Authorize(Roles = "Administrator")]
    public class ReportsController : ControllerBase
    {
        private readonly IAcademicProgramRepository _programRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IUserRepository _userRepo;
        private readonly IClassroomRepository _classroomRepo;
        private readonly IGraduationRepository _graduationRepo;

        public ReportsController(IAcademicProgramRepository programRepo, ICourseRepository courseRepo, IUserRepository userRepo,
                                 IClassroomRepository classroomRepo, IGraduationRepository graduationRepo)
        {
            _programRepo = programRepo;
            _courseRepo = courseRepo;
            _userRepo = userRepo;
            _classroomRepo = classroomRepo;
            _graduationRepo = graduationRepo;
        }

        [HttpGet("classrooms-without-teachers")]
        public async Task<ActionResult<IEnumerable<ClassroomDto>>> GetClassroomsWithoutTeachers()
        {
            var classrooms = await _classroomRepo.GetClassroomsWithoutTeacherAsync();
            var classroomsDto = classrooms.Select(c => new ClassroomDto
            {
                ClassroomId = c.ClassroomId,
                Name = c.Name,
                CourseId = c.CourseId,
                CourseName = c.Course.Name
            });
            return Ok(classroomsDto);
        }

        [HttpGet("courses-without-classrooms")]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesWithoutClassrooms()
        {
            var courses = await _courseRepo.GetCoursesWithoutClassroomsAsync();
            var coursesDto = courses.Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                Name = c.Name,
                AcademicProgramId = c.AcademicProgramId,
                AcademicProgramName = c.AcademicProgram?.Name ?? "N/A"
            });
            return Ok(coursesDto);
        }

        [HttpGet("enrollment-deficiencies/program/{programId}")]
        public async Task<ActionResult<EnrollmentDeficiencyReportDto>> GetEnrollmentDeficiencies(int programId)
        {
            var program = await _programRepo.GetProgramByIdAsync(programId);
            if (program == null) return NotFound(new { message = "لم يتم العثور على البرنامج." });

            var programCourses = (await _courseRepo.GetCoursesByProgramAsync(programId)).ToList();
            var programCourseIds = programCourses.Select(c => c.CourseId).ToHashSet();
            var studentsInProgram = await _userRepo.GetStudentsWithEnrollmentsByProgramAsync(programId);

            var report = new EnrollmentDeficiencyReportDto
            {
                ProgramId = programId,
                ProgramName = program.Name,
                StudentsWithDeficiencies = new List<StudentDeficiencyDto>()
            };

            foreach (var student in studentsInProgram)
            {
                var enrolledCourseIds = student.Enrollments
                                               .Select(e => e.Classroom.CourseId)
                                               .ToHashSet();

                var missingCourseIds = programCourseIds.Except(enrolledCourseIds);

                if (missingCourseIds.Any())
                {
                    var studentDeficiency = new StudentDeficiencyDto
                    {
                        StudentId = student.UserId,
                        StudentName = $"{student.FirstName} {student.LastName}",
                        MissingCourses = programCourses
                                        .Where(c => missingCourseIds.Contains(c.CourseId))
                                        .Select(c => new CourseShortDto { CourseId = c.CourseId, CourseName = c.Name })
                                        .ToList()
                    };
                    report.StudentsWithDeficiencies.Add(studentDeficiency);
                }
            }
            return Ok(report);
        }

        [HttpGet("graduates-pending-certificate")]
        public async Task<ActionResult<IEnumerable<GraduateDto>>> GetGraduatesPendingCertificate()
        {
            var graduates = await _graduationRepo.GetGraduatesPendingCertificateAsync();
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
                HasCertificate = false
            });
            return Ok(dtos);
        }
        [HttpGet("courses-without-coordinators")]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesWithoutCoordinators()
        {
            var courses = await _courseRepo.GetCoursesWithoutCoordinatorAsync();
            var coursesDto = courses.Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                Name = c.Name,
                AcademicProgramId = c.AcademicProgramId,
                AcademicProgramName = c.AcademicProgram?.Name ?? "N/A",
                CoordinatorId = null,
                CoordinatorName = "غير معين"
            });
            return Ok(coursesDto);
        }
    }
}