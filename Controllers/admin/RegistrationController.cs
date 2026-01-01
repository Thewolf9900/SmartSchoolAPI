using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/registrations")]
    [Authorize(Roles = "Administrator")]
    public class RegistrationController : ControllerBase
    {
        private readonly IProgramRegistrationRepository _registrationRepo;
        private readonly IUserRepository _userRepo;
        private readonly IFileService _fileService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;

        public RegistrationController(
            IProgramRegistrationRepository registrationRepo,
            IUserRepository userRepo,
            IFileService fileService,
            IEmailService emailService,
            IEmailTemplateService emailTemplateService)
        {
            _registrationRepo = registrationRepo;
            _userRepo = userRepo;
            _fileService = fileService;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }

        #region عرض وإدارة حالات الطلبات

        [HttpGet]
        public async Task<IActionResult> GetRegistrations([FromQuery] RegistrationStatus? status)
        {
            var registrations = await _registrationRepo.GetRegistrationsByStatusAsync(status);
            var dtos = registrations.Select(r => new {
                r.RegistrationId,
                FullName = $"{r.FirstName} {r.LastName}",
                r.Email,
                r.NationalId, // إضافة الرقم الوطني للعرض
                AcademicProgramName = r.AcademicProgram?.Name ?? "غير محدد",
                r.Status,
                r.RequestDate,
                PaymentReceiptUrl = r.PaymentReceiptUrl
            });
            return Ok(dtos);
        }

        [HttpPost("{id}/request-payment")]
        public async Task<IActionResult> RequestPayment(int id, [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] string adminNotes = null)
        {
            var registration = await _registrationRepo.GetRegistrationByIdAsync(id);
            if (registration == null) return NotFound();

            if (registration.Status != RegistrationStatus.PendingReview)
            {
                return BadRequest(new { message = "يمكن فقط تحويل الطلبات التي هي 'قيد المراجعة' إلى حالة انتظار الدفع." });
            }

            registration.Status = RegistrationStatus.AwaitingPayment;
            registration.AdminNotes = adminNotes;
            await _registrationRepo.SaveChangesAsync();
            return Ok(new { message = "تم تحديث حالة الطلب إلى 'في انتظار الدفع'." });
        }

        [HttpPost("{id}/request-new-receipt")]
        public async Task<IActionResult> RequestNewReceipt(int id, [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] string adminNotes)
        {
            if (string.IsNullOrWhiteSpace(adminNotes))
            {
                return BadRequest(new { message = "يجب تقديم سبب لرفض الإيصال." });
            }

            var registration = await _registrationRepo.GetRegistrationByIdAsync(id);
            if (registration == null) return NotFound(new { message = "لم يتم العثور על طلب التسجيل." });

            if (registration.Status != RegistrationStatus.PaymentSubmitted)
            {
                return BadRequest(new { message = "يمكن فقط رفض إيصال الطلبات التي تم تقديم دفعها." });
            }

            if (!string.IsNullOrEmpty(registration.PaymentReceiptPublicId))
            {
                await _fileService.DeleteFileAsync(registration.PaymentReceiptPublicId);
                registration.PaymentReceiptUrl = null;
                registration.PaymentReceiptPublicId = null;
            }

            registration.Status = RegistrationStatus.ReceiptRejected;
            registration.AdminNotes = adminNotes;
            await _registrationRepo.SaveChangesAsync();

            var subject = "ملاحظة بخصوص إيصال الدفع";
            var placeholders = new Dictionary<string, string>
            {
                { "FirstName", registration.FirstName },
                { "AdminNotes", adminNotes }
            };
            var body = await _emailTemplateService.GetTemplateAsync("RegistrationReceiptRejected.html", placeholders);
            await _emailService.SendEmailAsync(registration.Email, subject, body);

            return Ok(new { message = "تم تحديث حالة الطلب إلى 'الإيصال مرفوض' وإعلام المستخدم." });
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveRegistration(int id)
        {
            var registration = await _registrationRepo.GetRegistrationByIdAsync(id);
            if (registration == null) return NotFound(new { message = "لم يتم العثور על طلب التسجيل." });

            if (registration.Status != RegistrationStatus.PaymentSubmitted)
            {
                return BadRequest(new { message = "يمكن فقط الموافقة على الطلبات التي تم إرسال إيصال الدفع لها." });
            }

            // التحقق من وجود مستخدم فعلي بنفس الرقم الوطني
            if (await _userRepo.GetUserByNationalIdAsync(registration.NationalId) != null)
            {
                return Conflict(new { message = "يوجد مستخدم مسجل بالفعل بهذا الرقم الوطني." });
            }

            var newUser = new User
            {
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                Email = registration.Email,
                NationalId = registration.NationalId,
                Role = UserRole.Student,
                AcademicProgramId = registration.AcademicProgramId,
                PasswordHash = registration.PasswordHash
            };

            await _userRepo.CreateUserAsync(newUser);

            registration.Status = RegistrationStatus.Approved;
            registration.AdminNotes = $"تمت الموافقة وإنشاء حساب المستخدم ID: {newUser.UserId}";

            await _userRepo.SaveChangesAsync();

            var subject = "تهانينا! تم قبولك في Smart School";
            var programName = registration.AcademicProgram?.Name ?? "البرنامج الأكاديمي";
            var placeholders = new Dictionary<string, string>
            {
                { "FirstName", newUser.FirstName },
                { "ProgramName", programName }
            };
            var body = await _emailTemplateService.GetTemplateAsync("RegistrationApproved.html", placeholders);
            await _emailService.SendEmailAsync(newUser.Email, subject, body);

            return Ok(new { message = "تمت الموافقة على الطلب بنجاح، وتم إرسال بريد إلكتروني للطالب." });
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectRegistration(int id, [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] string adminNotes = null)
        {
            var registration = await _registrationRepo.GetRegistrationByIdAsync(id);
            if (registration == null) return NotFound(new { message = "لم يتم العثور על طلب التسجيل." });

            if (string.IsNullOrWhiteSpace(adminNotes))
            {
                return BadRequest(new { message = "يجب تقديم سبب للرفض." });
            }

            if (!string.IsNullOrEmpty(registration.PaymentReceiptPublicId))
            {
                await _fileService.DeleteFileAsync(registration.PaymentReceiptPublicId);
            }

            var subject = "بخصوص طلب تسجيلك في Smart School";
            var placeholders = new Dictionary<string, string>
            {
                { "FirstName", registration.FirstName },
                { "AdminNotes", adminNotes }
            };
            var body = await _emailTemplateService.GetTemplateAsync("RegistrationRejected.html", placeholders);
            await _emailService.SendEmailAsync(registration.Email, subject, body);

            _registrationRepo.DeleteRegistration(registration);
            await _registrationRepo.SaveChangesAsync();

            return Ok(new { message = "تم رفض وحذف طلب التسجيل بنجاح." });
        }

        #endregion
    }
}