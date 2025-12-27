using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Quiz;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Teacher
{
    [ApiController]
    [Route("api/teacher/quizzes")]
    [Authorize(Roles = "Teacher")]
    public class LectureQuizzesController : ControllerBase
    {
        private readonly ILectureQuizRepository _quizRepo;
        private readonly ILectureRepository _lectureRepo;
        private readonly IFileService _fileService;

        public LectureQuizzesController(ILectureQuizRepository quizRepo, ILectureRepository lectureRepo, IFileService fileService)
        {
            _quizRepo = quizRepo;
            _lectureRepo = lectureRepo;
            _fileService = fileService;
        }

        [HttpPost("lecture/{lectureId}")]
        public async Task<IActionResult> CreateQuizForLecture(int lectureId, [FromBody] CreateLectureQuizDto createDto)
        {
            var teacherId = GetCurrentUserId();
            var lecture = await _lectureRepo.GetLectureByIdAsync(lectureId);

            if (lecture == null) return NotFound(new { message = "المحاضرة غير موجودة." });
            if (!await IsTeacherOfLectureAsync(teacherId, lecture)) return Forbid();

            var existingQuiz = await _quizRepo.GetQuizByLectureIdAsync(lectureId);
            if (existingQuiz != null)
            {
                return BadRequest(new { message = "هذه المحاضرة تحتوي على اختبار بالفعل. لا يمكن إنشاء أكثر من اختبار واحد." });
            }

            var newQuiz = new LectureQuiz
            {
                Title = createDto.Title,
                LectureId = lectureId,
                IsEnabled = false
            };

            await _quizRepo.CreateQuizAsync(newQuiz);
            await _quizRepo.SaveChangesAsync();

            return Ok(new { newQuiz.LectureQuizId, newQuiz.Title, message = "تم إنشاء الاختبار بنجاح." });
        }

        [HttpGet("{quizId}/details")]
        public async Task<ActionResult<LectureQuizDetailsDto>> GetQuizDetails(int quizId)
        {
            var teacherId = GetCurrentUserId();
            var quiz = await _quizRepo.GetQuizWithDetailsByIdAsync(quizId);
            if (quiz == null) return NotFound(new { message = "الاختبار غير موجود." });

            var lecture = await _lectureRepo.GetLectureByIdAsync(quiz.LectureId);
            if (!await IsTeacherOfLectureAsync(teacherId, lecture)) return Forbid();

            var quizDto = new LectureQuizDetailsDto
            {
                LectureQuizId = quiz.LectureQuizId,
                Title = quiz.Title,
                LectureId = quiz.LectureId,
                IsEnabled = quiz.IsEnabled,
                CreatedAt = quiz.CreatedAt,
                Questions = quiz.Questions.Select(q => new QuizQuestionDto
                {
                    LectureQuizQuestionId = q.LectureQuizQuestionId,
                    Text = q.Text,
                    ImageUrl = q.ImageUrl,
                    QuestionType = q.QuestionType,
                    Options = q.Options.Select(o => new QuizQuestionOptionDto
                    {
                        LectureQuizQuestionOptionId = o.LectureQuizQuestionOptionId,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList()
            };

            return Ok(quizDto);
        }

        [HttpPost("{quizId}/toggle-status")]
        public async Task<IActionResult> ToggleQuizStatus(int quizId)
        {
            var teacherId = GetCurrentUserId();
            var quiz = await _quizRepo.GetQuizByIdAsync(quizId);
            if (quiz == null)
            {
                return NotFound(new { message = "الاختبار غير موجود." });
            }

            var lecture = await _lectureRepo.GetLectureByIdAsync(quiz.LectureId);
            if (!await IsTeacherOfLectureAsync(teacherId, lecture))
            {
                return Forbid();
            }

            // --- هنا التعديل ---
            // نقوم بعكس الحالة الحالية للاختبار
            quiz.IsEnabled = !quiz.IsEnabled;

            await _quizRepo.SaveChangesAsync();

            // نقوم بإرسال رسالة توضيحية بناءً على الحالة الجديدة
            var message = quiz.IsEnabled
                ? "تم تفعيل الاختبار بنجاح. لن يتمكن الطلاب من رؤيته ولن تتمكن من تعديله بعد الآن."
                : "تم إيقاف تفعيل الاختبار بنجاح. يمكنك الآن تعديله مرة أخرى.";

            // من الأفضل إرجاع الحالة الجديدة للواجهة الأمامية
            return Ok(new { message = message, isEnabled = quiz.IsEnabled });
        }

        [HttpPost("{quizId}/questions")]
        public async Task<IActionResult> AddQuestionToQuiz(int quizId, [FromForm] CreateQuizQuestionDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.Text) && (createDto.Image == null || createDto.Image.Length == 0))
                return BadRequest(new { message = "يجب توفير نص أو صورة للسؤال على الأقل." });

            var teacherId = GetCurrentUserId();
            var quiz = await _quizRepo.GetQuizByIdAsync(quizId);
            if (quiz == null) return NotFound(new { message = "الاختبار غير موجود." });
            var lecture = await _lectureRepo.GetLectureByIdAsync(quiz.LectureId);
            if (!await IsTeacherOfLectureAsync(teacherId, lecture)) return Forbid();

            if (quiz.IsEnabled)
            {
                return BadRequest(new { message = "لا يمكن إضافة أسئلة إلى اختبار تم تفعيله." });
            }

            if (createDto.Options == null || createDto.Options.Count < 2)
            {
                return BadRequest(new { message = "يجب توفير خيارين على الأقل للسؤال." });
            }
            var newQuestion = new LectureQuizQuestion
            {
                LectureQuizId = quizId,
                Text = createDto.Text,
                QuestionType = createDto.QuestionType,
                Options = createDto.Options.Select(o => new LectureQuizQuestionOption
                {
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToList()
            };

            if (createDto.Image != null && createDto.Image.Length > 0)
            {
                var uploadResult = await _fileService.SaveFileAsync(createDto.Image, "quiz_questions");
                newQuestion.ImageUrl = uploadResult.Url;
                newQuestion.ImagePublicId = uploadResult.PublicId;
            }

            await _quizRepo.CreateQuestionAsync(newQuestion);
            await _quizRepo.SaveChangesAsync();

            return Ok(new { message = "تمت إضافة السؤال بنجاح." });
        }

        [HttpPut("questions/{questionId}")]
        public async Task<IActionResult> UpdateQuestion(int questionId, [FromBody] UpdateQuizQuestionDto updateDto)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();

            var question = await _quizRepo.GetQuestionWithDetailsByIdAsync(questionId);
            if (question == null) return NotFound(new { message = "السؤال غير موجود." });

            var lecture = await _lectureRepo.GetLectureByIdAsync(question.LectureQuiz.LectureId);
            if (!await IsTeacherOfLectureAsync(teacherId, lecture)) return Forbid();

            if (question.LectureQuiz.IsEnabled)
            {
                return BadRequest(new { message = "لا يمكن تعديل سؤال في اختبار تم تفعيله." });
            }

            question.Text = updateDto.Text;
            question.QuestionType = updateDto.QuestionType;

            var optionsToDelete = question.Options
                .Where(o => !updateDto.Options.Any(dto => dto.LectureQuizQuestionOptionId == o.LectureQuizQuestionOptionId))
                .ToList();
            foreach (var option in optionsToDelete)
            {
                question.Options.Remove(option);
            }

            foreach (var dtoOption in updateDto.Options)
            {
                var existingOption = question.Options.FirstOrDefault(o => o.LectureQuizQuestionOptionId == dtoOption.LectureQuizQuestionOptionId);
                if (existingOption != null)
                {
                    existingOption.Text = dtoOption.Text;
                    existingOption.IsCorrect = dtoOption.IsCorrect;
                }
                else
                {
                    question.Options.Add(new LectureQuizQuestionOption
                    {
                        Text = dtoOption.Text,
                        IsCorrect = dtoOption.IsCorrect
                    });
                }
            }

            if (await _quizRepo.SaveChangesAsync())
            {
                return NoContent();
            }

            return BadRequest(new { message = "فشل في تحديث السؤال." });
        }

        [HttpDelete("questions/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var teacherId = GetCurrentUserId();
            var question = await _quizRepo.GetQuestionWithDetailsByIdAsync(questionId);
            if (question == null) return NoContent();

            var lecture = await _lectureRepo.GetLectureByIdAsync(question.LectureQuiz.LectureId);
            if (!await IsTeacherOfLectureAsync(teacherId, lecture)) return Forbid();

            if (question.LectureQuiz.IsEnabled)
            {
                return BadRequest(new { message = "لا يمكن حذف سؤال من اختبار تم تفعيله." });
            }

            if (!string.IsNullOrEmpty(question.ImagePublicId))
            {
                await _fileService.DeleteFileAsync(question.ImagePublicId);
            }

            _quizRepo.DeleteQuestion(question);
            await _quizRepo.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{quizId}")]
        public async Task<IActionResult> DeleteQuiz(int quizId)
        {
            var teacherId = GetCurrentUserId();
            var quiz = await _quizRepo.GetQuizWithDetailsByIdAsync(quizId);
            if (quiz == null) return NoContent();

            var lecture = await _lectureRepo.GetLectureByIdAsync(quiz.LectureId);
            if (!await IsTeacherOfLectureAsync(teacherId, lecture)) return Forbid();

            if (quiz.IsEnabled)
            {
                return BadRequest(new { message = "لا يمكن حذف اختبار تم تفعيله." });
            }

            foreach (var question in quiz.Questions)
            {
                if (!string.IsNullOrEmpty(question.ImagePublicId))
                {
                    await _fileService.DeleteFileAsync(question.ImagePublicId);
                }
            }

            _quizRepo.DeleteQuiz(quiz);
            await _quizRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{quizId}/submissions")]
        public async Task<ActionResult<IEnumerable<QuizSubmissionSummaryDto>>> GetQuizSubmissions(int quizId)
        {
            var teacherId = GetCurrentUserId();
            var quiz = await _quizRepo.GetQuizByIdAsync(quizId);
            if (quiz == null) return NotFound(new { message = "الاختبار غير موجود." });

            var lecture = await _lectureRepo.GetLectureByIdAsync(quiz.LectureId);
            if (!await IsTeacherOfLectureAsync(teacherId, lecture)) return Forbid();

            var submissions = await _quizRepo.GetSubmissionsForQuizAsync(quizId);

            var dtos = submissions.Select(s => new QuizSubmissionSummaryDto
            {
                StudentId = s.StudentId,
                StudentName = $"{s.Student.FirstName} {s.Student.LastName}",
                Score = s.Score,
                TotalQuestions = s.TotalQuestions,
                SubmittedAt = s.SubmittedAt
            });

            return Ok(dtos);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId)) return userId;
            return null;
        }

        private async Task<bool> IsTeacherOfLectureAsync(int? teacherId, Lecture lecture)
        {
            if (teacherId == null || lecture == null) return false;
            var classroom = await _lectureRepo.GetClassroomForLectureAsync(lecture.LectureId);
            return classroom != null && classroom.TeacherId == teacherId;
        }
    }
}