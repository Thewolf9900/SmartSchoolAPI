using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.User;
using SmartSchoolAPI.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/account")]
    [Authorize(Roles = "Administrator")]
    public class AccountController : ControllerBase
    {
        private readonly IUserRepository _userRepo;

        public AccountController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserDto>> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _userRepo.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { message = "لم يتم العثور على ملف المدير." });
            }

            var userDto = new UserDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                NationalId = user.NationalId,
                Role = user.Role
            };

            return Ok(userDto);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateAdminProfileDto updateDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _userRepo.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                 return NotFound(new { message = "لم يتم العثور على ملف المدير." });
            }

            if (user.NationalId != updateDto.CurrentNationalId)
            {
                return BadRequest(new { message = "فشل التحقق: الرقم الوطني الحالي غير صحيح." });
            }

            var existingUserWithEmail = await _userRepo.GetUserByEmailAsync(updateDto.Email);
            if (existingUserWithEmail != null && existingUserWithEmail.UserId != userId)
            {
                return BadRequest(new { message = "البريد الإلكتروني الجديد مستخدم بالفعل." });
            }

            var existingUserWithNationalId = await _userRepo.GetUserByNationalIdAsync(updateDto.NationalId);
            if (existingUserWithNationalId != null && existingUserWithNationalId.UserId != userId)
            {
                return BadRequest(new { message = "الرقم الوطني الجديد مستخدم بالفعل." });
            }

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.Email = updateDto.Email;
            user.NationalId = updateDto.NationalId;

            await _userRepo.SaveChangesAsync();

            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}