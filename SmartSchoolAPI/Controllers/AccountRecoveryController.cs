using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.AccountRecovery;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers
{
    [ApiController]
    [Route("api/account-recovery")]
    public class AccountRecoveryController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;

        public AccountRecoveryController(IUserRepository userRepo, IEmailService emailService)
        {
            _userRepo = userRepo;
            _emailService = emailService;
        }

         [HttpGet("find-email")]
        public async Task<IActionResult> FindEmail([FromQuery] string nationalId)
        {
            var user = await _userRepo.GetUserByNationalIdAsync(nationalId);
            if (user == null)
            {
                return NotFound(new { message = "No account found with this National ID." });
            }

              return Ok(new { email = user.Email });
        }

         [HttpPost("request-reset")]
        public async Task<IActionResult> RequestPasswordReset(RequestResetDto requestDto)
        {
            var user = await _userRepo.GetUserByEmailAsync(requestDto.Email);
            if (user == null || user.NationalId != requestDto.NationalId)
            {
                 return Ok(new { message = "If an account with this information exists, a password reset code has been sent." });
            }

             var random = new Random();
            var resetCode = random.Next(100000, 999999).ToString();

            var token = new PasswordResetToken
            {
                UserId = user.UserId,
                ResetCode = resetCode,
                ExpiryDate = DateTime.UtcNow.AddMinutes(15) 
            };
            await _userRepo.SetPasswordResetTokenAsync(token);
            await _userRepo.SaveChangesAsync();

             var subject = "Your Password Reset Code";
            var body = $"<p>Your password reset code is: <strong>{resetCode}</strong></p><p>This code will expire in 15 minutes.</p>";
            await _emailService.SendEmailAsync(user.Email, subject, body);

            return Ok(new { message = "إذا كان هناك حساب بهذه المعلومات، فسيتم إرسال رمز إعادة تعيين كلمة المرور." });
        }

         [HttpPost("confirm-reset")]
        public async Task<IActionResult> ConfirmPasswordReset(ConfirmResetDto confirmDto)
        {
            var user = await _userRepo.GetUserByEmailAsync(confirmDto.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid reset code or email." });
            }

            var token = await _userRepo.GetPasswordResetTokenByUserIdAsync(user.UserId);
            if (token == null || token.ResetCode != confirmDto.ResetCode || token.ExpiryDate <= DateTime.UtcNow)
            {
                return BadRequest(new { message = "رمز إعادة الضبط غير صالح أو منتهي الصلاحية." });
            }

             user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(confirmDto.NewPassword);
 
            await _userRepo.SaveChangesAsync();

            return Ok(new { message = "لقد تم إعادة تعيين كلمة المرور الخاصة بك بنجاح." });
        }
    }
}