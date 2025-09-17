 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.DTOs.Challenge;
using SmartSchoolAPI.Entities; 
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;  
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Student
{
    [ApiController]
    [Route("api/student/challenge")]
    [Authorize(Roles = "Student")]
    public class ChallengeController : ControllerBase
    {
        private readonly IQuestionRepository _questionRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly IWeeklyChallengeRepository _challengeRepo; 
        private readonly SmartSchoolAPI.Data.SmartSchoolDbContext _context;  

        private const int ChallengeQuestionCount = 10;

        public ChallengeController(
            IQuestionRepository questionRepo,
            ICourseRepository courseRepo,
            IEnrollmentRepository enrollmentRepo,
            IWeeklyChallengeRepository challengeRepo,
            SmartSchoolAPI.Data.SmartSchoolDbContext context)
        {
            _questionRepo = questionRepo;
            _courseRepo = courseRepo;
            _enrollmentRepo = enrollmentRepo;
            _challengeRepo = challengeRepo;  
            _context = context;  
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<ChallengeQuestionDto>>> GetWeeklyChallengeForCourse(int courseId)
        {
            var studentId = GetCurrentUserId();
            if (studentId == null) return Unauthorized();

            var isEnrolled = await _enrollmentRepo.IsStudentEnrolledInCourseAsync(studentId.Value, courseId);
            if (!isEnrolled) return Forbid();

             var (year, week) = GetCurrentWeekNumber();
            var existingSubmission = await _challengeRepo.GetSubmissionAsync(studentId.Value, courseId, year, week);
            if (existingSubmission != null)
            {
                return BadRequest(new { message = "لقد قمت بالفعل بإجراء تحدي هذا الأسبوع لهذا المساق." });
            }

            var questions = await _questionRepo.GetRandomApprovedQuestionsForCourseAsync(courseId, ChallengeQuestionCount);

            if (questions.Count < ChallengeQuestionCount) 
            {
                return NotFound(new { message = "لا توجد أسئلة معتمدة كافية في بنك الأسئلة لهذا المساق حاليًا." });
            }

            var challengeDtos = questions.Select(q => new ChallengeQuestionDto
            {
                QuestionId = q.QuestionId,
                Text = q.Text,
                ImageUrl = q.ImageUrl,
                Options = q.Options.Select(o => new ChallengeQuestionOptionDto
                {
                    QuestionOptionId = o.QuestionOptionId,
                    Text = o.Text
                }).ToList()
            }).ToList();

            return Ok(challengeDtos);
        }

         [HttpPost("course/{courseId}/submit")]
        public async Task<IActionResult> SubmitWeeklyChallenge(int courseId, [FromBody] SubmitChallengeDto submissionDto)
        {
            var studentId = GetCurrentUserId();
            if (studentId == null) return Unauthorized();

            var isEnrolled = await _enrollmentRepo.IsStudentEnrolledInCourseAsync(studentId.Value, courseId);
            if (!isEnrolled) return Forbid();

            var (year, week) = GetCurrentWeekNumber();
            var existingSubmission = await _challengeRepo.GetSubmissionAsync(studentId.Value, courseId, year, week);
            if (existingSubmission != null)
            {
                return BadRequest(new { message = "لقد قمت بالفعل بتقديم تحدي هذا الأسبوع." });
            }

             var questionIds = submissionDto.Answers.Select(a => a.QuestionId).ToList();
            var questionsFromDb = await _context.Questions.Include(q => q.Options)
                                           .Where(q => questionIds.Contains(q.QuestionId)).ToListAsync();

            if (questionIds.Count != questionsFromDb.Count)
            {
                return BadRequest(new { message = "بعض الأسئلة المقدمة غير صالحة." });
            }

            int score = 0;
            foreach (var answer in submissionDto.Answers)
            {
                var question = questionsFromDb.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
                var correctOption = question?.Options.FirstOrDefault(o => o.IsCorrect);
                if (correctOption != null && correctOption.QuestionOptionId == answer.SelectedOptionId)
                {
                    score++;
                }
            }

            var newSubmission = new WeeklyChallengeSubmission
            {
                StudentId = studentId.Value,
                CourseId = courseId,
                Year = year,
                WeekOfYear = week,
                Score = score,
                TimeTakenSeconds = submissionDto.TimeTakenSeconds,
                SubmittedAt = System.DateTime.UtcNow
            };

            await _challengeRepo.AddSubmissionAsync(newSubmission);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تقديم إجاباتك بنجاح!", yourScore = score });
        }
        [HttpGet("course/{courseId}/leaderboard")]
        public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetLeaderboard(int courseId)
        {
            var studentId = GetCurrentUserId();
            if (studentId == null) return Unauthorized();

             var isEnrolled = await _enrollmentRepo.IsStudentEnrolledInCourseAsync(studentId.Value, courseId);
            if (!isEnrolled) return Forbid();

            var (year, week) = GetCurrentWeekNumber();
            var submissions = await _challengeRepo.GetLeaderboardAsync(courseId, year, week);

            var leaderboard = submissions.Select((s, index) => new LeaderboardEntryDto
            {
                Rank = index + 1,  
                StudentId = s.StudentId,
                StudentName = $"{s.Student.FirstName} {s.Student.LastName}",
                
                Score = s.Score,
                TimeTakenSeconds = s.TimeTakenSeconds
            }).ToList();

            return Ok(leaderboard);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId)) return userId;
            return null;
        }

        private (int Year, int Week) GetCurrentWeekNumber()
        {
            var now = System.DateTime.UtcNow;
            var calendar = CultureInfo.InvariantCulture.Calendar;
            var weekOfYear = calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return (now.Year, weekOfYear);
        }


    }
}