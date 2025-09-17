using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Archive;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/archive")]
    [Authorize(Roles = "Administrator")]
    public class ArchiveController : ControllerBase
    {
        private readonly IArchiveRepository _archiveRepo;
        private readonly IArchiveService _archiveService;

        public ArchiveController(IArchiveRepository archiveRepo, IArchiveService archiveService)
        {
            _archiveRepo = archiveRepo;
            _archiveService = archiveService;
        }

        [HttpPost("classrooms/{classroomId}")]
        public async Task<IActionResult> ArchiveClassroom(int classroomId)
        {
            var (success, message) = await _archiveService.ArchiveClassroomAsync(classroomId);

            if (success)
            {
                return Ok(new { message });
            }

            return BadRequest(new { message });
        }

        [HttpGet("classrooms")]
        public async Task<ActionResult<IEnumerable<ArchivedClassroomDto>>> GetArchivedClassrooms()
        {
            var archivedClassrooms = await _archiveRepo.GetAllArchivedClassroomsAsync();

            var dtos = archivedClassrooms.Select(ac => new ArchivedClassroomDto
            {
                ArchivedClassroomId = ac.ArchivedClassroomId,
                OriginalClassroomId = ac.OriginalClassroomId,
                Name = ac.Name,
                CourseName = ac.CourseName,
                ProgramName = ac.ProgramName,
                TeacherName = ac.TeacherName,
                ArchivedAt = ac.ArchivedAt,
                EnrolledStudents = ac.ArchivedEnrollments.Select(ae => new ArchivedEnrollmentDto
                {
                    StudentName = ae.StudentName,
                    StudentNationalId = ae.StudentNationalId,
                    PracticalGrade = ae.PracticalGrade,
                    ExamGrade = ae.ExamGrade,
                    FinalGrade = ae.FinalGrade
                }).ToList()
            });

            return Ok(dtos);
        }
    }
}