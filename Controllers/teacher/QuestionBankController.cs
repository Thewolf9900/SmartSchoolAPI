using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.QuestionBank;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Teacher
{
    [ApiController]
    [Route("api/teacher/question-bank")]
    [Authorize(Roles = "Teacher")]
    public class QuestionBankController : ControllerBase
    {
        private readonly IQuestionRepository _questionRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IClassroomRepository _classroomRepo;
        private readonly IFileService _fileService;

        public QuestionBankController(IQuestionRepository questionRepo, ICourseRepository courseRepo, IClassroomRepository classroomRepo, IFileService fileService)
        {
            _questionRepo = questionRepo;
            _courseRepo = courseRepo;
            _classroomRepo = classroomRepo;
            _fileService = fileService;
        }

        #region عمليات المنسق (Coordinator)

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<QuestionDto>>> GetCourseQuestions(int courseId)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();

            var course = await _courseRepo.GetCourseByIdAsync(courseId);
            if (course == null) return NotFound(new { message = "الدورة غير موجودة." });

            if (course.CoordinatorId != teacherId)
            {
                return Forbid();
            }

            var questions = await _questionRepo.GetQuestionsByCourseAsync(courseId);
            var dtos = questions.Select(q => new QuestionDto
            {
                QuestionId = q.QuestionId,
                Text = q.Text,
                ImageUrl = q.ImageUrl,
                QuestionType = q.QuestionType,
                DifficultyLevel = q.DifficultyLevel,
                Status = q.Status,
                CreatedAt = q.CreatedAt,
                CreatedBy = $"{q.CreatedBy.FirstName} {q.CreatedBy.LastName}",
                ReviewedBy = q.ReviewedBy != null ? $"{q.ReviewedBy.FirstName} {q.ReviewedBy.LastName}" : null,
                Options = q.Options.Select(o => new QuestionOptionDto
                {
                    QuestionOptionId = o.QuestionOptionId,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToList()
            });
            return Ok(dtos);
        }

        [HttpPatch("questions/{questionId}/review")]
        public async Task<IActionResult> ReviewQuestion(int questionId, [FromBody] ReviewQuestionDto reviewDto)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();

            var question = await _questionRepo.GetQuestionByIdAsync(questionId);
            if (question == null) return NotFound(new { message = "السؤال غير موجود." });

            if (question.Course.CoordinatorId != teacherId)
            {
                return Forbid();
            }

            if (question.Status != QuestionStatus.Pending)
            {
                return BadRequest(new { message = $"لا يمكن مراجعة هذا السؤال لأنه في حالة '{question.Status}' حاليًا." });
            }

            question.Status = reviewDto.NewStatus;
            question.ReviewedById = teacherId.Value;
            question.ReviewedAt = DateTime.UtcNow;

            await _questionRepo.SaveChangesAsync();
            return Ok(new { message = $"تم تحديث حالة السؤال بنجاح إلى '{reviewDto.NewStatus}'." });
        }
        [HttpPatch("questions/{questionId}/revert-review")]
        public async Task<IActionResult> RevertQuestionReview(int questionId)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();

            var question = await _questionRepo.GetQuestionByIdAsync(questionId);
            if (question == null) return NotFound(new { message = "السؤال غير موجود." });

            if (question.Course.CoordinatorId != teacherId)
            {
                return Forbid();
            }

            if (question.Status == QuestionStatus.Pending)
            {
                return BadRequest(new { message = "هذا السؤال لا يزال قيد المراجعة بالفعل." });
            }

            question.Status = QuestionStatus.Pending;
            question.ReviewedById = null;
            question.ReviewedAt = null;

            await _questionRepo.SaveChangesAsync();

            return Ok(new { message = "تمت إعادة السؤال إلى حالة 'قيد المراجعة' بنجاح." });
        }

        #endregion

        #region عمليات مشتركة (المدرس والمنسق)

        [HttpGet("course/{courseId}/my-suggestions")]
        public async Task<ActionResult<IEnumerable<QuestionDto>>> GetMyPendingSuggestions(int courseId)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();

            var isAssociated = await _classroomRepo.IsTeacherAssociatedWithCourseAsync(teacherId.Value, courseId);
            if (!isAssociated)
            {
                var course = await _courseRepo.GetCourseByIdAsync(courseId);
                if (course == null || course.CoordinatorId != teacherId)
                {
                    if (!isAssociated) return Forbid();
                }
            }

            var questions = await _questionRepo.GetPendingSuggestionsByAuthorAsync(courseId, teacherId.Value);

            var dtos = questions.Select(q => new QuestionDto
            {
                QuestionId = q.QuestionId,
                Text = q.Text,
                ImageUrl = q.ImageUrl,
                QuestionType = q.QuestionType,
                DifficultyLevel = q.DifficultyLevel,
                Status = q.Status,
                CreatedAt = q.CreatedAt,
                CreatedBy = "أنا",
                ReviewedBy = null,
                Options = q.Options.Select(o => new QuestionOptionDto
                {
                    QuestionOptionId = o.QuestionOptionId,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToList()
            });

            return Ok(dtos);
        }

        [HttpPost("course/{courseId}/suggest")]
        public async Task<IActionResult> SuggestQuestionForCourse(int courseId, [FromForm] CreateQuestionDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.Text) && (createDto.Image == null || createDto.Image.Length == 0))
                return BadRequest(new { message = "يجب توفير نص أو صورة للسؤال على الأقل." });
            if (createDto.Options == null || createDto.Options.Count < 2)
                return BadRequest(new { message = "يجب توفير خيارين على الأقل." });

            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();

            var course = await _courseRepo.GetCourseByIdAsync(courseId);
            if (course == null) return NotFound(new { message = "الدورة المحددة غير موجودة." });

            var isAssociated = await _classroomRepo.IsTeacherAssociatedWithCourseAsync(teacherId.Value, courseId);
            if (!isAssociated && course.CoordinatorId != teacherId) return Forbid();

            var newQuestion = new Question
            {
                Text = createDto.Text,
                QuestionType = createDto.QuestionType,
                DifficultyLevel = createDto.DifficultyLevel,
                CourseId = courseId,
                CreatedById = teacherId.Value,
                Options = createDto.Options.Select(o => new QuestionOption
                {
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToList()
            };

            if (createDto.Image != null && createDto.Image.Length > 0)
            {
                var uploadResult = await _fileService.SaveFileAsync(createDto.Image, "question_bank");
                newQuestion.ImageUrl = uploadResult.Url;
                newQuestion.ImagePublicId = uploadResult.PublicId;
            }

            if (course.CoordinatorId == teacherId)
            {
                newQuestion.Status = QuestionStatus.Approved;
                newQuestion.ReviewedById = teacherId.Value;
                newQuestion.ReviewedAt = DateTime.UtcNow;
            }
            else
            {
                newQuestion.Status = QuestionStatus.Pending;
            }

            await _questionRepo.CreateQuestionAsync(newQuestion);
            await _questionRepo.SaveChangesAsync();

            return Ok(new { message = "تم إرسال السؤال بنجاح." });
        }

        [HttpPut("questions/{questionId}")]
        public async Task<IActionResult> UpdateQuestion(int questionId, [FromForm] UpdateQuestionDto updateDto)
        {
            if (string.IsNullOrWhiteSpace(updateDto.Text) && (updateDto.NewImage == null || updateDto.NewImage.Length == 0) && !updateDto.DeleteCurrentImage)
            {
                var questionForCheck = await _questionRepo.GetQuestionByIdAsync(questionId);
                if (questionForCheck != null && string.IsNullOrEmpty(questionForCheck.ImageUrl))
                    return BadRequest(new { message = "يجب توفير نص للسؤال إذا لم تكن هناك صورة حالية أو جديدة." });
            }

            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();

            var question = await _questionRepo.GetQuestionByIdAsync(questionId);
            if (question == null) return NotFound(new { message = "السؤال غير موجود." });

            bool isCoordinator = question.Course.CoordinatorId == teacherId;
            bool isCreatorOfPending = question.CreatedById == teacherId && question.Status == QuestionStatus.Pending;

            if (!isCoordinator && !isCreatorOfPending) return Forbid();

            if (updateDto.DeleteCurrentImage && !string.IsNullOrEmpty(question.ImagePublicId))
            {
                await _fileService.DeleteFileAsync(question.ImagePublicId);
                question.ImageUrl = null;
                question.ImagePublicId = null;
            }
            else if (updateDto.NewImage != null && updateDto.NewImage.Length > 0)
            {
                if (!string.IsNullOrEmpty(question.ImagePublicId))
                {
                    await _fileService.DeleteFileAsync(question.ImagePublicId);
                }
                var uploadResult = await _fileService.SaveFileAsync(updateDto.NewImage, "question_bank");
                question.ImageUrl = uploadResult.Url;
                question.ImagePublicId = uploadResult.PublicId;
            }

            question.Text = updateDto.Text;
            question.QuestionType = updateDto.QuestionType;
            question.DifficultyLevel = updateDto.DifficultyLevel;

            var optionsToDelete = question.Options.Where(o => !updateDto.Options.Any(dto => dto.QuestionOptionId == o.QuestionOptionId)).ToList();
            foreach (var option in optionsToDelete) question.Options.Remove(option);

            foreach (var dtoOption in updateDto.Options)
            {
                var existingOption = question.Options.FirstOrDefault(o => o.QuestionOptionId == dtoOption.QuestionOptionId);
                if (existingOption != null)
                {
                    existingOption.Text = dtoOption.Text;
                    existingOption.IsCorrect = dtoOption.IsCorrect;
                }
                else
                {
                    question.Options.Add(new QuestionOption { Text = dtoOption.Text, IsCorrect = dtoOption.IsCorrect });
                }
            }

            if (isCoordinator)
            {
                question.ReviewedById = teacherId;
                question.ReviewedAt = DateTime.UtcNow;
            }

            if (await _questionRepo.SaveChangesAsync())
            {
                return NoContent();
            }

            return BadRequest(new { message = "فشل في تحديث السؤال." });
        }

        [HttpDelete("questions/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();

            var question = await _questionRepo.GetQuestionByIdAsync(questionId);
            if (question == null) return NotFound();

            bool isCoordinator = question.Course.CoordinatorId == teacherId;
            bool isCreatorOfPending = question.CreatedById == teacherId && question.Status == QuestionStatus.Pending;

            if (!isCoordinator && !isCreatorOfPending)
            {
                return Forbid();
            }

            if (!string.IsNullOrEmpty(question.ImagePublicId))
            {
                await _fileService.DeleteFileAsync(question.ImagePublicId);
            }

            _questionRepo.DeleteQuestion(question);
            await _questionRepo.SaveChangesAsync();

            return NoContent();
        }

        #endregion

        #region الدوال المساعدة

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId)) return userId;
            return null;
        }

        #endregion
    }
}