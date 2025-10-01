using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.DTOs.Public;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers
{
    [ApiController]
    [Route("api/public")]
    [AllowAnonymous]
    public class PublicController : ControllerBase
    {
        private readonly IAcademicProgramRepository _programRepo;
        private readonly IProgramRegistrationRepository _registrationRepo;
        private readonly IFileService _fileService;
        private readonly SmartSchoolDbContext _context;
        private readonly IUserRepository _userRepo;

        public PublicController(
            IAcademicProgramRepository programRepo,
            IProgramRegistrationRepository registrationRepo,
            IFileService fileService,
            SmartSchoolDbContext context,
            IUserRepository userRepo)
        {
            _programRepo = programRepo;
            _registrationRepo = registrationRepo;
            _fileService = fileService;
            _context = context;
            _userRepo = userRepo;
        }

        #region استكشاف البرامج والتسجيل

        [HttpGet("programs")]
        public async Task<IActionResult> GetAvailablePrograms()
        {
            var programs = await _programRepo.GetProgramsOpenForRegistrationAsync();

            var dtos = programs.Select(p => new PublicProgramDto
            {
                AcademicProgramId = p.AcademicProgramId,
                Name = p.Name,
                Description = p.Description,
                CourseNames = p.Courses.Select(c => c.Name).ToList(),
                TotalPrice = p.Courses.Sum(c => c.Price)
            });

            return Ok(dtos);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateRegistrationDto dto)
        {
            // --- هنا المنطق الجديد والمحكم ---

            // 1. التحقق من وجود مستخدم فعلي
            if (await _userRepo.GetUserByNationalIdAsync(dto.NationalId) != null || await _userRepo.GetUserByEmailAsync(dto.Email) != null)
            {
                return Conflict(new { message = "هذه البيانات مسجلة بالفعل في النظام. إذا كنت طالباً حالياً، يرجى التسجيل في برنامج جديد من خلال لوحة التحكم الخاصة بك." });
            }

            // 2. التحقق من وجود طلب تسجيل نشط
            if (await _registrationRepo.GetActiveRegistrationByNationalIdAsync(dto.NationalId) != null || await _registrationRepo.GetActiveRegistrationByEmailAsync(dto.Email) != null)
            {
                return Conflict(new { message = "لديك بالفعل طلب تسجيل قيد المعالجة." });
            }

            // 3. التحقق من البرنامج والسماح بإعادة التسجيل بعد الرفض
            var program = await _programRepo.GetProgramByIdAsync(dto.AcademicProgramId);
            if (program == null || !program.IsRegistrationOpen)
            {
                return BadRequest(new { message = "التسجيل في هذا البرنامج غير متاح حاليًا." });
            }

            // حذف أي طلبات قديمة مرفوضة لنفس الشخص ونفس البرنامج
            var existingRejectedRegistration = await _registrationRepo.GetRegistrationByEmailAndProgramAsync(dto.Email, dto.AcademicProgramId);
            if (existingRejectedRegistration != null && existingRejectedRegistration.Status == RegistrationStatus.Rejected)
            {
                _registrationRepo.DeleteRegistration(existingRejectedRegistration);
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var newRegistration = new ProgramRegistration
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                NationalId = dto.NationalId,
                PasswordHash = hashedPassword,
                AcademicProgramId = dto.AcademicProgramId,
                Status = RegistrationStatus.PendingReview,
                RequestDate = System.DateTime.UtcNow
            };

            await _registrationRepo.AddRegistrationAsync(newRegistration);
            await _registrationRepo.SaveChangesAsync();

            return Ok(new { message = "تم استلام طلب التسجيل الخاص بك بنجاح. سيتم مراجعته من قبل الإدارة." });
        }

        #endregion

        #region متابعة حالة الطلب ورفع الإيصال

        [HttpGet("registrations/status")]
        public async Task<IActionResult> GetRegistrationStatus([FromQuery] string email, [FromQuery] int programId)
        {
            var registration = await _registrationRepo.GetRegistrationByEmailAndProgramAsync(email, programId);
            if (registration == null)
            {
                return NotFound(new { message = "لم يتم العثور على طلب تسجيل مطابق." });
            }

            var statusDto = new RegistrationStatusDto
            {
                RegistrationId = registration.RegistrationId,
                FullName = $"{registration.FirstName} {registration.LastName}",
                ProgramName = registration.AcademicProgram.Name,
                Status = registration.Status,
                RequestDate = registration.RequestDate,
                AdminNotes = registration.AdminNotes
            };

            if (registration.Status == RegistrationStatus.AwaitingPayment || registration.Status == RegistrationStatus.ReceiptRejected)
            {
                statusDto.PaymentDetails = await _context.PaymentSettings.FindAsync(1);
            }

            return Ok(statusDto);
        }

        [HttpPost("registrations/{id}/upload-receipt")]
        public async Task<IActionResult> UploadReceipt(int id)
        {
            if (Request.Form.Files.Count == 0)
            {
                return BadRequest(new { message = "يجب إرفاق ملف إيصال الدفع." });
            }
            var receiptFile = Request.Form.Files[0];

            var registration = await _registrationRepo.GetRegistrationByIdAsync(id);
            if (registration == null)
            {
                return NotFound(new { message = "لم يتم العثور على طلب التسجيل." });
            }

            if (registration.Status != RegistrationStatus.AwaitingPayment && registration.Status != RegistrationStatus.ReceiptRejected)
            {
                return Forbid("لا يمكنك رفع إيصال دفع لهذا الطلب في حالته الحالية.");
            }

            if (!string.IsNullOrEmpty(registration.PaymentReceiptPublicId))
            {
                await _fileService.DeleteFileAsync(registration.PaymentReceiptPublicId);
            }

            var uploadResult = await _fileService.SaveFileAsync(receiptFile, "payment-receipts");

            registration.PaymentReceiptUrl = uploadResult.Url;
            registration.PaymentReceiptPublicId = uploadResult.PublicId;
            registration.Status = RegistrationStatus.PaymentSubmitted;
            registration.AdminNotes = null;

            await _registrationRepo.SaveChangesAsync();

            return Ok(new { message = "تم رفع إيصال الدفع بنجاح. سيتم تدقيقه من قبل الإدارة قريبًا." });
        }

        #endregion
    }
}