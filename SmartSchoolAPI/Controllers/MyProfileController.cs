using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.User;
using SmartSchoolAPI.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers
{
    [ApiController]
    [Route("api/my-profile")] 
    [Authorize]
    public class MyProfileController : ControllerBase
    {
        private readonly IUserRepository _userRepo;

        public MyProfileController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet]  
        public async Task<ActionResult<UserDto>> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _userRepo.GetUserWithProgramByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { message = "لم يتم العثور على الملف الشخصي." });
            }

            var userDto = new UserDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                NationalId = user.NationalId,
                Role = user.Role,
                AcademicProgramName = user.AcademicProgram?.Name
            };

            return Ok(userDto);
        }

        [HttpPut("change-password")] 
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _userRepo.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { message = "لم يتم العثور على المستخدم." });
            }

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.OldPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "كلمة المرور الحالية غير صحيحة." });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

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