using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.User;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;

namespace SmartSchoolAPI.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IAcademicProgramRepository _programRepo;
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly IClassroomRepository _classroomRepo;

        public UsersController(IUserRepository userRepo, IAcademicProgramRepository programRepo, IEnrollmentRepository enrollmentRepo, IClassroomRepository classroomRepo)
        {
            _userRepo = userRepo;
            _programRepo = programRepo;
            _enrollmentRepo = enrollmentRepo;
            _classroomRepo = classroomRepo;
        }

         [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery] UserRole? role)
        {
            var users = await _userRepo.GetAllUsersAsync(role);

            var usersDto = users.Select(u => new UserDto
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                NationalId = u.NationalId,
                Role = u.Role
            });

            return Ok(usersDto);
        }

         [HttpGet("{id}", Name = "GetUserById")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var userDto = new UserDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role
            };
            return Ok(userDto);
        }


        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            var existingUser = await _userRepo.GetUserByEmailAsync(createUserDto.Email);
            if (existingUser != null)
            {
                return BadRequest("Email address is already in use.");
            }

            var userEntity = new User
            {
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Email = createUserDto.Email,
                Role = createUserDto.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
                NationalId = createUserDto.NationalId
            };

            await _userRepo.CreateUserAsync(userEntity);
            await _userRepo.SaveChangesAsync();

            var userToReturn = new UserDto
            {
                UserId = userEntity.UserId,
                FirstName = userEntity.FirstName,
                LastName = userEntity.LastName,
                Email = userEntity.Email,
                Role = userEntity.Role,
                NationalId = userEntity.NationalId
            };

            return CreatedAtRoute("GetUserById", new { id = userEntity.UserId }, userToReturn);
        }


        [HttpGet("/api/courses/{courseId}/teachers")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetTeachersForCourse(int courseId)
        {
             var teachers = await _userRepo.GetTeachersByCourseAsync(courseId);

            // سنعيد استخدام UserDto
            var teachersDto = teachers.Select(u => new UserDto
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                NationalId = u.NationalId,
                Role = u.Role
            });

            return Ok(teachersDto);
        }




        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserDto updateUserDto)
        {
            var userFromRepo = await _userRepo.GetUserByIdAsync(userId);
            if (userFromRepo == null)
            {
                return NotFound("User not found.");
            }

             var existingUserWithEmail = await _userRepo.GetUserByEmailAsync(updateUserDto.Email);
            if (existingUserWithEmail != null && existingUserWithEmail.UserId != userId)
            {
                return BadRequest("Email is already in use by another user.");
            }
 
            userFromRepo.FirstName = updateUserDto.FirstName;
            userFromRepo.LastName = updateUserDto.LastName;
            userFromRepo.Email = updateUserDto.Email;
            userFromRepo.NationalId = updateUserDto.NationalId;
            userFromRepo.Role = updateUserDto.Role;
 
            if (await _userRepo.SaveChangesAsync())
            {
                return NoContent();
            }

            return BadRequest("Failed to update user.");
        }
         [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userFromRepo = await _userRepo.GetUserByIdAsync(id);
            if (userFromRepo == null)
            {
                return NotFound();
            }
             if (userFromRepo.Role == UserRole.Student)
            {
                 var studentEnrollments = await _enrollmentRepo.GetEnrollmentsForStudentAsync(id);
                if (studentEnrollments.Any())
                {
                    return BadRequest("لا يمكن حذف هذا الطالب لأنه مسجل حاليًا في فصل دراسي واحد أو أكثر. يُرجى إلغاء تسجيله أولًا.");
                }
            }

             if (userFromRepo.Role == UserRole.Teacher)
            {
                 var taughtClassrooms = await _classroomRepo.GetClassroomsByTeacherAsync(id);
                if (taughtClassrooms.Any())
                {
                    return BadRequest("لا يمكن حذف هذا المعلم لأنه مُعيَّن حاليًا في فصل دراسي واحد أو أكثر. يُرجى إلغاء تعيينه أولًا.");
                }
            }

 
            _userRepo.DeleteUser(userFromRepo);
            if (await _userRepo.SaveChangesAsync())
            {
                return NoContent(); 
            }

            return BadRequest("فشل في حذف المستخدم");
        }




        [HttpPost("{userId}/reset-password")]
        public async Task<IActionResult> ResetUserPassword(int userId, [FromBody] ResetPasswordDto resetDto)
        {
             var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

             if (user.NationalId != resetDto.NationalId)
            {
                return BadRequest("إن رقم الهوية الوطنية المقدم لا يتطابق مع سجل المستخدم.");
            }
 
             var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);

             user.PasswordHash = newPasswordHash;

            // 5. حفظ التغييرات
            if (await _userRepo.SaveChangesAsync())
            {
                return Ok("تم إعادة تعيين كلمة مرور المستخدم بنجاح.");
            }

            return BadRequest("فشل إعادة تعيين كلمة مرور المستخدم.");
        }
    }
}
