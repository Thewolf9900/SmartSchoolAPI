using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Course;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/courses")]
    [Authorize(Roles = "Administrator")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseRepository _courseRepo;
        private readonly IAcademicProgramRepository _programRepo;
        private readonly IClassroomRepository _classroomRepo;

        public CoursesController(ICourseRepository courseRepo, IAcademicProgramRepository programRepo, IClassroomRepository classroomRepo)
        {
            _courseRepo = courseRepo;
            _programRepo = programRepo;
            _classroomRepo = classroomRepo;
        }

        #region عمليات قراءة الدورات

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
        {
            var courses = await _courseRepo.GetAllCoursesAsync();
            var coursesDto = courses.Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                Name = c.Name,
                Price = c.Price, 
                AcademicProgramId = c.AcademicProgramId,
                AcademicProgramName = c.AcademicProgram?.Name ?? "N/A",
                CoordinatorId = c.CoordinatorId,
                CoordinatorName = c.Coordinator != null ? $"{c.Coordinator.FirstName} {c.Coordinator.LastName}" : "غير معين"
            });
            return Ok(coursesDto);
        }

        [HttpGet("{id}", Name = "GetCourseById")]
        public async Task<ActionResult<CourseDto>> GetCourseById(int id)
        {
            var course = await _courseRepo.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound(new { message = $"لم يتم العثور على دورة بالمعرّف {id}." });
            }
            var courseDto = new CourseDto
            {
                CourseId = course.CourseId,
                Name = course.Name,
                Price = course.Price, 
                AcademicProgramId = course.AcademicProgramId,
                AcademicProgramName = course.AcademicProgram?.Name ?? "N/A",
                CoordinatorId = course.CoordinatorId,
                CoordinatorName = course.Coordinator != null ? $"{course.Coordinator.FirstName} {course.Coordinator.LastName}" : "غير معين"
            };
            return Ok(courseDto);
        }

        #endregion

        #region عمليات إنشاء وتعديل وحذف الدورات

        [HttpPost]
        public async Task<ActionResult<CourseDto>> CreateCourse(CreateCourseDto createDto)
        {
            var program = await _programRepo.GetProgramByIdAsync(createDto.AcademicProgramId);
            if (program == null)
            {
                return BadRequest(new { message = "البرنامج الأكاديمي المحدد غير موجود." });
            }

            var courseEntity = new Course
            {
                Name = createDto.Name,
                AcademicProgramId = createDto.AcademicProgramId,
                Price = createDto.Price,  
                CreatedAt = System.DateTime.UtcNow
            };

            await _courseRepo.CreateCourseAsync(courseEntity);
            await _courseRepo.SaveChangesAsync();

            var courseToReturn = new CourseDto
            {
                CourseId = courseEntity.CourseId,
                Name = courseEntity.Name,
                Price = courseEntity.Price,
                AcademicProgramId = courseEntity.AcademicProgramId,
                AcademicProgramName = program.Name,
                CoordinatorId = courseEntity.CoordinatorId,
                CoordinatorName = courseEntity.Coordinator != null ? $"{courseEntity.Coordinator.FirstName} {courseEntity.Coordinator.LastName}" : "غير معين"
            };
            return CreatedAtAction(nameof(GetCourseById), new { id = courseEntity.CourseId }, courseToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, UpdateCourseDto updateDto)
        {
            var courseFromRepo = await _courseRepo.GetCourseByIdAsync(id);
            if (courseFromRepo == null)
            {
                return NotFound(new { message = "لم يتم العثور على الدورة." });
            }

            courseFromRepo.Name = updateDto.Name;
            courseFromRepo.Price = updateDto.Price;  

            await _courseRepo.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var classroomsInCourse = await _classroomRepo.GetClassroomsByCourseAsync(id);
            if (classroomsInCourse.Any())
            {
                return BadRequest(new { message = "لا يمكن حذف هذه الدورة لأنها تحتوي على فصول مرتبطة بها. يرجى حذف الفصول أولاً." });
            }

            var courseFromRepo = await _courseRepo.GetCourseByIdAsync(id);
            if (courseFromRepo == null)
            {
                return NotFound(new { message = "لم يتم العثور على الدورة." });
            }

            _courseRepo.DeleteCourse(courseFromRepo);
            await _courseRepo.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/assign-coordinator")]
        public async Task<IActionResult> AssignCoordinatorToCourse(int id, [FromBody] AssignCoordinatorDto assignDto)
        {
            var (success, errorMessage) = await _courseRepo.AssignCoordinatorAsync(id, assignDto.TeacherId);

            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            if (await _courseRepo.SaveChangesAsync())
            {
                return NoContent();
            }

            return BadRequest(new { message = "حدث خطأ أثناء حفظ التغييرات." });
        }

        [HttpDelete("{id}/unassign-coordinator")]
        public async Task<IActionResult> UnassignCoordinatorFromCourse(int id)
        {
            var success = await _courseRepo.UnassignCoordinatorAsync(id);
            if (!success)
            {
                return NotFound(new { message = "الدورة غير موجودة." });
            }

            if (await _courseRepo.SaveChangesAsync())
            {
                return NoContent();
            }

            return BadRequest(new { message = "حدث خطأ أثناء حفظ التغييرات." });
        }
        #endregion
    }
}