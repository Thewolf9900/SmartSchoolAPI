using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Alumni;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers
{
    [ApiController]
    [Route("api/public/alumni")]
    public class AlumniController : ControllerBase
    {
        private readonly IGraduationRepository _graduationRepo;

        public AlumniController(IGraduationRepository graduationRepo)
        {
            _graduationRepo = graduationRepo;
        }

        [HttpGet("records")]
        public async Task<ActionResult<IEnumerable<AlumniRecordDto>>> FindMyRecords([FromQuery] string nationalId)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
            {
                return BadRequest(new { message = "الرقم الوطني مطلوب." });
            }

            var graduationRecords = await _graduationRepo.GetGraduationsByNationalIdAsync(nationalId);

            var recordsDto = graduationRecords.Select(g => new AlumniRecordDto
            {
                GraduationId = g.GraduationId,
                FirstName = g.FirstName,
                LastName = g.LastName,
                ProgramNameAtGraduation = g.ProgramNameAtGraduation,
                GraduationDate = g.GraduationDate,
                FinalGpa = g.FinalGpa,
                HasCertificate = g.Certificate != null
            });

            return Ok(recordsDto);
        }
        [HttpGet("certificate/{graduationId}")]
        public async Task<IActionResult> DownloadCertificate(int graduationId)
        {
        
            var graduationRecord = await _graduationRepo.GetGraduationByIdAsync(graduationId);

             if (graduationRecord == null || graduationRecord.Certificate == null)
            {
                return NotFound(new { message = "لم يتم العثور على الشهادة المطلوبة." });
            }

             var certificate = graduationRecord.Certificate;
            return File(certificate.CertificateData, certificate.FileType, certificate.FileName);
        }
    }
}