using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Alumni;
using SmartSchoolAPI.Interfaces;
using System; 
using System.Collections.Generic;
using System.Linq;
using System.Net.Http; 
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers
{
    [ApiController]
    [Route("api/public/alumni")]
    public class AlumniController : ControllerBase
    {
        private readonly IGraduationRepository _graduationRepo;
        private readonly IHttpClientFactory _httpClientFactory;  

        public AlumniController(IGraduationRepository graduationRepo, IHttpClientFactory httpClientFactory) // تم تعديله
        {
            _graduationRepo = graduationRepo;
            _httpClientFactory = httpClientFactory;  
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

            if (graduationRecord?.Certificate == null || string.IsNullOrWhiteSpace(graduationRecord.Certificate.CertificateUrl))
            {
                return NotFound(new { message = "لم يتم العثور على الشهادة المطلوبة." });
            }

            var certificate = graduationRecord.Certificate;

            try
            {
                var client = _httpClientFactory.CreateClient();
                var fileBytes = await client.GetByteArrayAsync(certificate.CertificateUrl);
                return File(fileBytes, certificate.FileType, certificate.FileName);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "فشل تحميل ملف الشهادة من الخدمة السحابية." });
            }
        }
    }
}