using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.AcademicRecord;
using SmartSchoolAPI.DTOs.Announcement;
using SmartSchoolAPI.DTOs.Classroom;
using SmartSchoolAPI.DTOs.Lecture;
using SmartSchoolAPI.DTOs.Material;
using SmartSchoolAPI.DTOs.Quiz;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Student
{
    [ApiController]
    [Route("api/student")]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {

        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly IClassroomRepository _classroomRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IMaterialRepository _materialRepo;
        private readonly IAnnouncementRepository _announcementRepo;
        private readonly IUserRepository _userRepo;
        private readonly IFileService _fileService;
        private readonly IGraduationRepository _graduationRepo;
        private readonly ILectureQuizRepository _quizRepo;
        private readonly ILectureRepository _lectureRepo;
        private readonly IHttpClientFactory _httpClientFactory;

        public StudentController(
            IEnrollmentRepository enrollmentRepo, IClassroomRepository classroomRepo, ICourseRepository courseRepo,
            IMaterialRepository materialRepo, IUserRepository userRepo, IAnnouncementRepository announcementRepo,
            IFileService fileService, IGraduationRepository graduationRepo, ILectureRepository lectureRepo,
            ILectureQuizRepository quizRepo, IHttpClientFactory httpClientFactory)
        {
            _enrollmentRepo = enrollmentRepo;
            _classroomRepo = classroomRepo;
            _courseRepo = courseRepo;
            _materialRepo = materialRepo;
            _userRepo = userRepo;
            _announcementRepo = announcementRepo;
            _fileService = fileService;
            _graduationRepo = graduationRepo;
            _lectureRepo = lectureRepo;
            _quizRepo = quizRepo;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("my-classrooms")]
        public async Task<ActionResult<IEnumerable<StudentClassroomListDto>>> GetMyClassrooms()
        {
            var studentId = GetCurrentUserId();
            if (studentId == null) return Unauthorized();

            var enrollments = await _enrollmentRepo.GetEnrollmentsForStudentAsync(studentId.Value);

            var classroomDtos = enrollments.Select(e => new ClassroomDto
            {
                ClassroomId = e.Classroom.ClassroomId,
                Name = e.Classroom.Name,
                CourseName = e.Classroom.Course.Name,
                CourseId = e.Classroom.Course.CourseId,
                TeacherName = e.Classroom.Teacher != null ? $"{e.Classroom.Teacher.FirstName} {e.Classroom.Teacher.LastName}" : "غير معين",
                Status = e.Classroom.Status.ToString(),
                Capacity = e.Classroom.Capacity,
                EnrolledStudentsCount = e.Classroom.Enrollments.Count,
            }).ToList();

            return Ok(classroomDtos);
        }

        [HttpGet("classrooms/{classroomId}/details")]
        public async Task<ActionResult<StudentClassroomDetailsDto>> GetClassroomDetails(int classroomId)
        {
            var studentId = GetCurrentUserId();
            if (studentId == null) return Unauthorized();

            var classroom = await _classroomRepo.GetClassroomDetailsForStudentAsync(classroomId);

            if (classroom == null)
            {
                return NotFound(new { message = $"لم يتم العثور على فصل بالمعرّف {classroomId}." });
            }

            var studentEnrollment = classroom.Enrollments.FirstOrDefault(e => e.StudentId == studentId.Value);
            if (studentEnrollment == null)
            {
                return Forbid();
            }

            var quizIds = classroom.Lectures
                .Where(l => l.LectureQuiz != null)
                .Select(l => l.LectureQuiz.LectureQuizId)
                .ToList();

            var studentSubmissions = new List<LectureQuizSubmission>();
            if (quizIds.Any())
            {
                studentSubmissions = await _quizRepo.GetSubmissionsByStudentAndQuizzesAsync(studentId.Value, quizIds);
            }

            var submissionsMap = studentSubmissions.ToDictionary(s => s.LectureQuizId);

            var classroomDetailsDto = new StudentClassroomDetailsDto
            {
                ClassroomId = classroom.ClassroomId,
                ClassroomName = classroom.Name,
                CourseId = classroom.CourseId,
                CourseName = classroom.Course.Name,
                TeacherName = classroom.Teacher != null ? $"{classroom.Teacher.FirstName} {classroom.Teacher.LastName}" : "غير معين",
                Status = classroom.Status,
                PracticalGrade = studentEnrollment.PracticalGrade,
                ExamGrade = studentEnrollment.ExamGrade,
                FinalGrade = studentEnrollment.FinalGrade,
                Lectures = classroom.Lectures.Select(l => new LectureContentDto
                {
                    LectureId = l.LectureId,
                    Title = l.Title,
                    Description = l.Description,
                    LectureOrder = l.LectureOrder,
                    CreatedAt = l.CreatedAt,
                    Materials = l.Materials.Select(m => new MaterialDto
                    {
                        MaterialId = m.MaterialId,
                        Title = m.Title,
                        OriginalFilename = m.OriginalFilename,
                        Description = m.Description,
                        FileSize = m.FileSize,
                        UploadedAt = m.UploadedAt,
                        MaterialType = m.MaterialType,
                        Url = m.MaterialType == "Link" ? m.Url : null
                    }).ToList(),
                    LectureQuiz = l.LectureQuiz == null ? null : new LectureQuizSummaryDto
                    {
                        LectureQuizId = l.LectureQuiz.LectureQuizId,
                        Title = l.LectureQuiz.Title,
                        IsEnabled = l.LectureQuiz.IsEnabled,
                        IsSubmitted = submissionsMap.ContainsKey(l.LectureQuiz.LectureQuizId),
                        SubmissionId = submissionsMap.TryGetValue(l.LectureQuiz.LectureQuizId, out var submission)
                                        ? submission.LectureQuizSubmissionId
                                        : (int?)null
                    }
                }).OrderBy(l => l.LectureOrder).ToList()
            };

            return Ok(classroomDetailsDto);
        }

        [HttpGet("courses/{courseId}/materials")]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetCourseReferenceMaterials(int courseId)
        {
            var studentId = GetCurrentUserId();
            if (studentId == null) return Unauthorized();

            var course = await _courseRepo.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return NotFound(new { message = $"لم يتم العثور على دورة بالمعرّف {courseId}." });
            }

            var student = await _userRepo.GetUserByIdAsync(studentId.Value);
            if (student == null || student.AcademicProgramId != course.AcademicProgramId)
            {
                return Forbid();
            }

            var materials = await _materialRepo.GetMaterialsForCourseAsync(courseId);

            var materialDtos = materials.Select(m => new MaterialDto
            {
                MaterialId = m.MaterialId,
                Title = m.Title,
                Description = m.Description,
                MaterialType = m.MaterialType,
                OriginalFilename = m.OriginalFilename,
                Url = m.Url,
                FileSize = m.FileSize,
                UploadedAt = m.UploadedAt
            });

            return Ok(materialDtos);
        }

        //[HttpGet("materials/{materialId}/download")]
        //public async Task<IActionResult> DownloadMaterial(int materialId)
        //{
        //    var studentId = GetCurrentUserId();
        //    if (studentId == null) return Unauthorized();

        //    var material = await _materialRepo.GetMaterialWithDeepDetailsAsync(materialId);

        //    if (material == null || material.MaterialType != "File" || string.IsNullOrEmpty(material.Url))
        //    {
        //        return NotFound(new { message = "الملف غير موجود أو غير صالح للتحميل." });
        //    }

        //    bool hasAccess = false;

        //    if (material.Lecture?.Classroom != null)
        //    {
        //        hasAccess = material.Lecture.Classroom.Enrollments.Any(e => e.StudentId == studentId.Value);
        //    }
        //    else if (material.Course != null)
        //    {
        //        hasAccess = await _enrollmentRepo.IsStudentEnrolledInCourseAsync(studentId.Value, material.Course.CourseId);
        //    }

        //    if (!hasAccess)
        //    {
        //        return Forbid();
        //    }

        //    try
        //    {
        //        var client = _httpClientFactory.CreateClient();
        //        var fileBytes = await client.GetByteArrayAsync(material.Url);
        //        var downloadName = material.OriginalFilename ?? Path.GetFileName(new Uri(material.Url).LocalPath);

        //        var contentType = "application/octet-stream";
        //        if (downloadName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) contentType = "application/pdf";

        //        return File(fileBytes, contentType, downloadName);
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(500, new { message = "فشل تحميل الملف من الخدمة السحابية." });
        //    }
        //}

        [HttpGet("announcements")]
        public async Task<ActionResult<IEnumerable<AnnouncementDto>>> GetMyAnnouncements()
        {
            var studentId = GetCurrentUserId();
            if (studentId == null) return Unauthorized();

            var student = await _userRepo.GetUserByIdAsync(studentId.Value);
            var enrollments = await _enrollmentRepo.GetEnrollmentsForStudentAsync(studentId.Value);

            if (student == null || student.AcademicProgramId == null)
            {
                return Ok(new List<AnnouncementDto>());
            }

            var programId = student.AcademicProgramId.Value;
            var classroomIds = enrollments.Select(e => e.ClassroomId).ToList();
            var courseIds = enrollments.Select(e => e.Classroom.CourseId).Distinct().ToList();

            var announcements = await _announcementRepo.GetAnnouncementsForStudentAsync(programId, courseIds, classroomIds);

            var announcementDtos = announcements.Select(a => new AnnouncementDto
            {
                AnnouncementId = a.AnnouncementId,
                Title = a.Title,
                Content = a.Content,
                PostedAt = a.PostedAt,
                TargetScope = a.TargetScope,
                TargetId = a.AcademicProgramId ?? a.CourseId ?? a.ClassroomId,
                TargetName = a.AcademicProgram?.Name ?? a.Course?.Name ?? a.Classroom?.Name
            }).ToList();

            return Ok(announcementDtos);
        }


        [HttpGet("academic-record")]
        public async Task<ActionResult<StudentAcademicRecordDto>> GetMyAcademicRecord()
        {
            var studentId = GetCurrentUserId();
            if (studentId == null) return Unauthorized();

            var academicRecord = new StudentAcademicRecordDto();

            var graduationRecord = await _graduationRepo.GetGraduationByStudentIdAsync(studentId.Value);
            if (graduationRecord != null)
            {
                academicRecord.OverallStatus = AcademicStatus.Graduated;
                academicRecord.FinalGpa = graduationRecord.FinalGpa;
                academicRecord.CompletionDate = graduationRecord.GraduationDate;
                academicRecord.ProgramNameAtCompletion = graduationRecord.ProgramNameAtGraduation;
            }
            else
            {
                var failureRecord = await _graduationRepo.GetFailureByStudentIdAsync(studentId.Value);
                if (failureRecord != null)
                {
                    academicRecord.OverallStatus = AcademicStatus.Failed;
                    academicRecord.FinalGpa = failureRecord.FinalGpa;
                    academicRecord.CompletionDate = failureRecord.FailureDate;
                    academicRecord.ProgramNameAtCompletion = failureRecord.ProgramNameAtFailure;
                }
                else
                {
                    academicRecord.OverallStatus = AcademicStatus.Active;
                }
            }

            var enrollments = await _enrollmentRepo.GetEnrollmentsForStudentAsync(studentId.Value);
            academicRecord.EnrollmentHistory = enrollments.Select(e => new EnrollmentRecordDto
            {
                CourseName = e.Classroom.Course.Name,
                ClassroomName = e.Classroom.Name,
                PracticalGrade = e.PracticalGrade,
                ExamGrade = e.ExamGrade,
                FinalGrade = e.FinalGrade,
                ClassroomStatus = e.Classroom.Status.ToString()
            }).ToList();

            return Ok(academicRecord);
        }

        #region Lecture Quizzes Endpoints

        [HttpGet("lectures/{lectureId}/quiz")]
        public async Task<ActionResult<StudentQuizDto>> GetQuizForLecture(int lectureId)
        {
            var studentId = GetCurrentUserId();
            if (studentId == null) return Unauthorized();

            var lecture = await _lectureRepo.GetLectureByIdAsync(lectureId);
            if (lecture == null) return NotFound(new { message = "المحاضرة غير موجودة." });

            bool isEnrolled = await _enrollmentRepo.IsStudentEnrolledAsync(studentId.Value, lecture.ClassroomId);
            if (!isEnrolled) return Forbid();

            var quiz = await _quizRepo.GetQuizByLectureIdAsync(lectureId);
            if (quiz == null || !quiz.IsEnabled) return NotFound(new { message = "لا يوجد اختبار متاح لهذه المحاضرة حاليًا." });

            var existingSubmission = await _quizRepo.GetSubmissionAsync(quiz.LectureQuizId, studentId.Value);
            if (existingSubmission != null) return BadRequest(new { message = "لقد قمت بتقديم هذا الاختبار بالفعل." });

            var studentQuizDto = new StudentQuizDto
            {
                LectureQuizId = quiz.LectureQuizId,
                Title = quiz.Title,

                Questions = quiz.Questions.Select(q => new StudentQuizQuestionDto
                {
                    LectureQuizQuestionId = q.LectureQuizQuestionId,
                    Text = q.Text,
                    ImageUrl = q.ImageUrl,
                    Options = q.Options.Select(o => new StudentQuizOptionDto
                    {
                        LectureQuizQuestionOptionId = o.LectureQuizQuestionOptionId,
                        Text = o.Text
                    }).ToList()
                }).ToList()
            };

            return Ok(studentQuizDto);
        }

        [HttpPost("quizzes/{quizId}/submit")]
        public async Task<ActionResult<QuizResultDto>> SubmitQuiz(int quizId, [FromBody] SubmitQuizDto submissionDto)
        {
            var studentId = GetCurrentUserId();
            if (studentId == null) return Unauthorized();

            var quiz = await _quizRepo.GetQuizWithDetailsByIdAsync(quizId);
            if (quiz == null) return NotFound(new { message = "الاختبار غير موجود." });

            var lecture = await _lectureRepo.GetLectureByIdAsync(quiz.LectureId);
            if (lecture == null) return NotFound(new { message = "المحاضرة المرتبطة بالاختبار غير موجودة." });

            bool isEnrolled = await _enrollmentRepo.IsStudentEnrolledAsync(studentId.Value, lecture.ClassroomId);
            if (!isEnrolled) return Forbid();

            var existingSubmission = await _quizRepo.GetSubmissionAsync(quizId, studentId.Value);
            if (existingSubmission != null) return BadRequest(new { message = "لقد قمت بتقديم هذا الاختبار بالفعل." });

            var submission = new LectureQuizSubmission
            {
                LectureQuizId = quizId,
                StudentId = studentId.Value,
                TotalQuestions = quiz.Questions.Count,
                SubmittedAt = DateTime.UtcNow
            };

            int score = 0;
            var questionReviews = new List<QuestionReviewDto>();
            var studentAnswersMap = submissionDto.Answers.ToDictionary(a => a.QuestionId);

            foreach (var question in quiz.Questions)
            {
                var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
                bool wasCorrect = false;
                string studentAnswerText = "لم تتم الإجابة";

                if (studentAnswersMap.TryGetValue(question.LectureQuizQuestionId, out var studentAnswerDto))
                {
                    var selectedOption = question.Options.FirstOrDefault(o => o.LectureQuizQuestionOptionId == studentAnswerDto.SelectedOptionId);
                    studentAnswerText = selectedOption?.Text ?? "خيار غير صالح";

                    if (correctOption != null && selectedOption?.LectureQuizQuestionOptionId == correctOption.LectureQuizQuestionOptionId)
                    {
                        score++;
                        wasCorrect = true;
                    }

                    var studentAnswerEntity = new StudentAnswer
                    {
                        LectureQuizQuestionId = question.LectureQuizQuestionId,
                        SelectedOptionId = studentAnswerDto.SelectedOptionId,
                        Submission = submission
                    };
                    submission.StudentAnswers.Add(studentAnswerEntity);
                }

                questionReviews.Add(new QuestionReviewDto
                {
                    QuestionId = question.LectureQuizQuestionId,
                    QuestionText = question.Text,
                    YourAnswerText = studentAnswerText,
                    CorrectAnswerText = correctOption?.Text ?? "لا توجد إجابة صحيحة محددة",
                    WasCorrect = wasCorrect
                });
            }

            submission.Score = score;

            await _quizRepo.AddSubmissionAsync(submission);
            await _quizRepo.SaveChangesAsync();

            return Ok(new QuizResultDto
            {
                SubmissionId = submission.LectureQuizSubmissionId,
                StudentId = submission.StudentId,
                Score = submission.Score,
                TotalQuestions = submission.TotalQuestions,
                Message = "تم تقديم إجاباتك بنجاح!",
                QuestionReviews = questionReviews
            });
        }
        [HttpGet("submissions/{submissionId}/review")]
        public async Task<ActionResult<QuizReviewDetailsDto>> GetSubmissionReview(int submissionId)
        {
            var studentId = GetCurrentUserId();
            if (studentId == null) return Unauthorized();

            var submission = await _quizRepo.GetSubmissionForReviewAsync(submissionId);
            if (submission == null)
            {
                return NotFound(new { message = "لم يتم العثور على هذا التقديم." });
            }

            if (submission.StudentId != studentId.Value)
            {
                return Forbid();
            }

            var questionReviews = submission.LectureQuiz.Questions.Select(question =>
            {
                var studentAnswer = submission.StudentAnswers
                    .FirstOrDefault(sa => sa.LectureQuizQuestionId == question.LectureQuizQuestionId);

                var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);

                var selectedOption = studentAnswer?.SelectedOption;

                return new QuestionReviewDto
                {
                    QuestionId = question.LectureQuizQuestionId,
                    ImageUrl = question.ImageUrl,
                    QuestionText = question.Text,
                    YourAnswerText = selectedOption?.Text ?? "لم تتم الإجابة",
                    YourAnswerOptionId = selectedOption?.LectureQuizQuestionOptionId ?? 0,
                    CorrectAnswerText = correctOption?.Text ?? "لا توجد إجابة صحيحة محددة",
                    WasCorrect = selectedOption != null && selectedOption.IsCorrect
                };
            }).ToList();


            var reviewDto = new QuizReviewDetailsDto
            {
                SubmissionId = submission.LectureQuizSubmissionId,
                QuizTitle = submission.LectureQuiz.Title,
                Score = submission.Score,
                TotalQuestions = submission.TotalQuestions,
                SubmittedAt = submission.SubmittedAt,
                QuestionReviews = questionReviews
            };

            return Ok(reviewDto);
        }

        #endregion

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