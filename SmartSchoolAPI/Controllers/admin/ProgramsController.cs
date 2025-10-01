using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.AcademicProgram;
using SmartSchoolAPI.DTOs.Course;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/programs")]
    [Authorize(Roles = "Administrator")]
    public class ProgramsController : ControllerBase
    {
        private readonly IAcademicProgramRepository _programRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IUserRepository _userRepo;

        public ProgramsController(IAcademicProgramRepository programRepo, ICourseRepository courseRepo, IUserRepository userRepo)
        {
            _programRepo = programRepo;
            _courseRepo = courseRepo;
            _userRepo = userRepo;
        }

        #region عمليات قراءة البرامج

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProgramDto>>> GetPrograms()
        {
            var programs = await _programRepo.GetAllProgramsAsync();
            var programsDto = programs.Select(p => new ProgramDto
            {
                AcademicProgramId = p.AcademicProgramId,
                Name = p.Name,
                Description = p.Description,
                  IsRegistrationOpen = p.IsRegistrationOpen
            });
            return Ok(programsDto);
        }

        [HttpGet("{id}", Name = "GetProgramById")]
        public async Task<ActionResult<ProgramDto>> GetProgramById(int id)
        {
            var program = await _programRepo.GetProgramByIdAsync(id);
            if (program == null)
            {
                return NotFound(new { message = $"لم يتم العثور على برنامج بالمعرّف {id}." });
            }
            var programDto = new ProgramDto
            {
                AcademicProgramId = program.AcademicProgramId,
                Name = program.Name,
                Description = program.Description,
                IsRegistrationOpen = program.IsRegistrationOpen
            };
            return Ok(programDto);
        }

        [HttpGet("{programId}/courses")]
        [Authorize(Roles = "Administrator,Teacher,Student")]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesForProgram(int programId)
        {
            if (await _programRepo.GetProgramByIdAsync(programId) == null)
            {
                return NotFound(new { message = "لم يتم العثور على البرنامج." });
            }

            var courses = await _courseRepo.GetCoursesByProgramAsync(programId);
            var coursesDto = courses.Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                Name = c.Name,
                AcademicProgramId = c.AcademicProgramId,
                AcademicProgramName = c.AcademicProgram?.Name ?? "N/A"
            });
            return Ok(coursesDto);
        }

        #endregion

        #region عمليات إنشاء وتعديل وحذف البرامج

        [HttpPost]
        public async Task<ActionResult<ProgramDto>> CreateProgram([FromBody] CreateProgramDto createDto)
        {
            var programEntity = new AcademicProgram
            {
                Name = createDto.Name,
                Description = createDto.Description,
                CreatedAt = System.DateTime.UtcNow,
                IsRegistrationOpen = false 
                // التسجيل مغلق بشكل افتراضي عند إنشاء برنامج جديد
            };

            await _programRepo.CreateProgramAsync(programEntity);
            await _programRepo.SaveChangesAsync();

            var programToReturn = new ProgramDto
            {
                AcademicProgramId = programEntity.AcademicProgramId,
                Name = programEntity.Name,
                Description = programEntity.Description
            };

            return CreatedAtAction(nameof(GetProgramById), new { id = programEntity.AcademicProgramId }, programToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProgram(int id, [FromBody] UpdateProgramDto updateDto)
        {
            var programFromRepo = await _programRepo.GetProgramByIdAsync(id);
            if (programFromRepo == null)
            {
                return NotFound(new { message = "لم يتم العثور على البرنامج." });
            }

            programFromRepo.Name = updateDto.Name;
            programFromRepo.Description = updateDto.Description;

            await _programRepo.SaveChangesAsync();
            return NoContent();
        }

         [HttpPost("{id}/toggle-registration")]
        public async Task<IActionResult> ToggleRegistrationStatus(int id)
        {
            var program = await _programRepo.GetProgramByIdAsync(id);
            if (program == null)
            {
                return NotFound(new { message = "لم يتم العثور على البرنامج." });
            }

             program.IsRegistrationOpen = !program.IsRegistrationOpen;

            await _programRepo.SaveChangesAsync();

            var statusMessage = program.IsRegistrationOpen ? "مفتوح الآن" : "مغلق الآن";
            return Ok(new
            {
                message = $"تم تحديث حالة التسجيل للبرنامج بنجاح. الحالة الجديدة: {statusMessage}.",
                isRegistrationOpen = program.IsRegistrationOpen
            });
        }
 
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProgram(int id)
        {
            var coursesInProgram = await _courseRepo.GetCoursesByProgramAsync(id);
            if (coursesInProgram.Any())
            {
                return BadRequest(new { message = "لا يمكن حذف هذا البرنامج لأنه يحتوي على دورات مرتبطة به. يرجى حذف الدورات أولاً." });
            }

            var studentsInProgram = await _userRepo.GetStudentsByProgramAsync(id);
            if (studentsInProgram.Any())
            {
                return BadRequest(new { message = "لا يمكن حذف هذا البرنامج لأنه يوجد طلاب معينون له. يرجى إلغاء تعيينهم أولاً." });
            }

            var programFromRepo = await _programRepo.GetProgramByIdAsync(id);
            if (programFromRepo == null)
            {
                return NotFound(new { message = "لم يتم العثور على البرنامج." });
            }

            _programRepo.DeleteProgram(programFromRepo);
            await _programRepo.SaveChangesAsync();
            return NoContent();
        }

        #endregion
    }
}