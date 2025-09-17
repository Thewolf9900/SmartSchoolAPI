using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.Enums;
using System;
using System.Linq;
using System.Security.Claims;

namespace SmartSchoolAPI.Controllers
{
    [ApiController]
    [Route("api/options")]
    [Authorize] 
    public class OptionsController : ControllerBase
    {
        // GET: api/options/user-roles
        [HttpGet("user-roles")]
        [Authorize(Roles = "Administrator")] 
        public IActionResult GetUserRoles()
        {
            var roles = Enum.GetNames(typeof(UserRole));
            return Ok(roles);
        }

         [HttpGet("announcement-scopes")]
        [Authorize(Roles = "Administrator, Teacher")]
        public IActionResult GetAnnouncementScopes()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == "Administrator")
            {
                 var allScopes = Enum.GetNames(typeof(AnnouncementScope));
                return Ok(allScopes);
            }
            else // Teacher
            {
                 var teacherScopes = new[] { nameof(AnnouncementScope.CLASSROOM) };
                return Ok(teacherScopes);
            }
        }

         [HttpGet("classroom-statuses")]
        [Authorize(Roles = "Administrator")] 
        public IActionResult GetClassroomStatuses()
        {
            var statuses = Enum.GetNames(typeof(ClassroomStatus));
            return Ok(statuses);
        }
    }
}