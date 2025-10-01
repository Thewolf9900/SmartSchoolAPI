using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartSchoolAPI.DTOs.Auth;
using SmartSchoolAPI.DTOs.User;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IProgramRegistrationRepository _registrationRepo;
        private readonly IConfiguration _config;

        public AuthController(IUserRepository userRepo, IConfiguration config, IProgramRegistrationRepository registrationRepo)
        {
            _userRepo = userRepo;
            _config = config;
            _registrationRepo = registrationRepo;
        }

        [HttpPost("login")]
        [AllowAnonymous] // يجب أن تكون متاحة للعامة
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            // 1. البحث في جدول المستخدمين الفعليين
            var user = await _userRepo.GetUserByEmailAsync(loginDto.Email);
            if (user != null)
            {
                if (BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    var token = GenerateJwtToken(user);
                    return Ok(new CheckStatusResponseDto
                    {
                        UserType = user.Role.ToString(),
                        Token = token,
                        FullName = $"{user.FirstName} {user.LastName}"
                    });
                }
            }

            // 2. البحث في جدول طلبات التسجيل
            var registration = await _registrationRepo.GetPendingRegistrationByEmailAsync(loginDto.Email);
            if (registration != null)
            {
                if (BCrypt.Net.BCrypt.Verify(loginDto.Password, registration.PasswordHash))
                {
                    return Ok(new CheckStatusResponseDto
                    {
                        UserType = "Applicant",
                        ApplicantStatus = registration.Status,
                        RegistrationId = registration.RegistrationId,
                        FullName = $"{registration.FirstName} {registration.LastName}"
                    });
                }
            }

            // 3. إذا لم يتم العثور على أي شيء
            return Unauthorized(new { message = "البريد الإلكتروني أو كلمة المرور غير صحيحة." });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}