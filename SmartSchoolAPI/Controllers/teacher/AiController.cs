using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.QuestionBank;
using SmartSchoolAPI.Interfaces;
using System.Security.Claims;
using System; // Required for Exception handling
using System.Collections.Generic; // Required for IEnumerable
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.teacher
{
    [ApiController]
    [Route("api/teacher/ai")]
    [Authorize(Roles = "Teacher")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly IFileService _fileService;
        private readonly ICourseRepository _courseRepo;
        private readonly IClassroomRepository _classroomRepo;

        public AiController(
            IAiService aiService,
            IFileService fileService,
            ICourseRepository courseRepo,
            IClassroomRepository classroomRepo)
        {
            _aiService = aiService;
            _fileService = fileService;
            _courseRepo = courseRepo;
            _classroomRepo = classroomRepo;
        }

        /// <summary>
        /// يولد أسئلة بناءً على محتوى نصي.
        /// </summary>
        /// <param name="language">لغة المخرجات المطلوبة (e.g., "Arabic", "English"). يتم تمريرها كـ query parameter.</param>
        [HttpPost("course/{courseId}/generate-from-text")]
        public async Task<ActionResult<IEnumerable<CreateQuestionDto>>> GenerateQuestionsFromText(int courseId, [FromBody] GenerateQuestionsFromTextDto generationDto, [FromQuery] string language = "Arabic")
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();

            if (!await IsTeacherAuthorizedForCourse(teacherId.Value, courseId))
            {
                return Forbid("أنت لست مخولاً بالوصول إلى هذه الدورة.");
            }

            try
            {
                // يتم الآن تمرير اللغة بشكل صريح إلى الخدمة
                var generatedQuestions = await _aiService.GenerateQuestionsAsync(generationDto, language);
                return Ok(generatedQuestions);
            }
            catch (Exception ex)
            {
                // Log the exception ex
                return StatusCode(500, new { message =ex });
            }
        }

        /// <summary>
        /// يولد أسئلة بناءً على ملف مرفوع.
        /// </summary>
        /// <param name="language">لغة المخرجات المطلوبة (e.g., "Arabic", "English"). يتم تمريرها كـ query parameter.</param>
        [HttpPost("course/{courseId}/generate-from-file")]
        public async Task<ActionResult<IEnumerable<CreateQuestionDto>>> GenerateQuestionsFromFile(int courseId, [FromForm] GenerateQuestionsFromFileDto generationDto, [FromQuery] string language = "Arabic")
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();

            if (!await IsTeacherAuthorizedForCourse(teacherId.Value, courseId))
            {
                return Forbid("أنت لست مخولاً بالوصول إلى هذه الدورة.");
            }

            try
            {
                var fileContent = await _fileService.ReadFileContentAsync(generationDto.File);
                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    return BadRequest(new { message = "الملف فارغ أو لا يمكن قراءة محتواه كنص." });
                }

                var textGenerationDto = new GenerateQuestionsFromTextDto
                {
                    ContextText = fileContent,
                    NumberOfQuestions = generationDto.NumberOfQuestions,
                    Difficulty = generationDto.Difficulty,
                    QuestionType = generationDto.QuestionType
                };

                // يتم الآن تمرير اللغة بشكل صريح إلى الخدمة
                var generatedQuestions = await _aiService.GenerateQuestionsAsync(textGenerationDto, language);
                return Ok(generatedQuestions);
            }
            catch (Exception ex)
            {
                // Log the exception ex
                return StatusCode(500, new { message = "حدث خطأ غير متوقع أثناء معالجة الملف وتوليد الأسئلة." });
            }
        }


        #region الدوال المساعدة
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId)) return userId;
            return null;
        }

        private async Task<bool> IsTeacherAuthorizedForCourse(int teacherId, int courseId)
        {
            var course = await _courseRepo.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return false;
            }
            if (course.CoordinatorId == teacherId)
            {
                return true;
            }
            var isAssociated = await _classroomRepo.IsTeacherAssociatedWithCourseAsync(teacherId, courseId);
            return isAssociated;
        }
        #endregion
    }
}