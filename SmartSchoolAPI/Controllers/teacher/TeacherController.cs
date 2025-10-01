using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSchoolAPI.DTOs.Announcement;
using SmartSchoolAPI.DTOs.Challenge;
using SmartSchoolAPI.DTOs.Classroom;
using SmartSchoolAPI.DTOs.Course;
using SmartSchoolAPI.DTOs.Enrollment;
using SmartSchoolAPI.DTOs.Lecture;
using SmartSchoolAPI.DTOs.Material;
using SmartSchoolAPI.DTOs.Quiz;
using SmartSchoolAPI.DTOs.Reports;
using SmartSchoolAPI.DTOs.User;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.teacher
{
    [ApiController]
    [Route("api/teacher")]
    [Authorize(Roles = "Teacher")]
    public class TeacherController : ControllerBase
    {
        private readonly IClassroomRepository _classroomRepository;
        private readonly ILectureRepository _lectureRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly IFileService _fileService;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IWeeklyChallengeRepository _challengeRepo;

        public TeacherController(
            IClassroomRepository classroomRepository, ILectureRepository lectureRepository,
            IMaterialRepository materialRepository, IFileService fileService,
            IEnrollmentRepository enrollmentRepository, IAnnouncementRepository announcementRepository,
            ICourseRepository courseRepository, IWeeklyChallengeRepository challengeRepo)
        {
            _classroomRepository = classroomRepository;
            _lectureRepository = lectureRepository;
            _materialRepository = materialRepository;
            _fileService = fileService;
            _enrollmentRepository = enrollmentRepository;
            _announcementRepository = announcementRepository;
            _courseRepository = courseRepository;
            _challengeRepo = challengeRepo;
        }

        #region نظرة عامة ووظائف المنسق

        [HttpGet("my-coordinated-courses")]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetMyCoordinatedCourses()
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();

            var courses = await _courseRepository.GetCoursesByCoordinatorIdAsync(teacherId.Value);

            var dtos = courses.Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                Name = c.Name,
                AcademicProgramId = c.AcademicProgramId,
                AcademicProgramName = c.AcademicProgram.Name
            });

            return Ok(dtos);
        }

        #endregion

        #region إدارة الفصول ونظرة عامة

        [HttpGet("classrooms")]
        public async Task<IActionResult> GetMyClassrooms([FromQuery] ClassroomStatus? status)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized(new { message = "معرّف المستخدم غير صالح أو مفقود في التوكن." });

            var classrooms = await _classroomRepository.GetClassroomsByTeacherIdAsync(teacherId.Value, status);

            var classroomDtos = classrooms.Select(c => new ClassroomDto
            {
                ClassroomId = c.ClassroomId,
                Name = c.Name,
                CourseId = c.CourseId,
                CourseName = c.Course.Name,
                AcademicProgramId = c.Course.AcademicProgram.AcademicProgramId,
                AcademicProgramName = c.Course.AcademicProgram.Name,
                TeacherId = c.TeacherId,
                TeacherName = (c.Teacher != null) ? $"{c.Teacher.FirstName} {c.Teacher.LastName}" : "غير معين",
                Capacity = c.Capacity,
                Status = c.Status.ToString(),
                EnrolledStudentsCount = c.Enrollments.Count
            }).ToList();

            return Ok(classroomDtos);
        }

        [HttpGet("classrooms/{classroomId}/details")]
        public async Task<IActionResult> GetClassroomDetails(int classroomId)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized(new { message = "معرّف المستخدم غير صالح." });

            var classroom = await _classroomRepository.GetClassroomWithDetailsForTeacherPortalAsync(classroomId);
            if (classroom == null) return NotFound(new { message = $"لم يتم العثور على فصل بالمعرّف {classroomId}." });
            if (classroom.TeacherId != teacherId) return Forbid();

            var announcements = await _announcementRepository.GetLatestAnnouncementsForClassroomAsync(classroomId, 5);

            var classroomDetailsDto = new ClassroomDetailsDto
            {
                ClassroomId = classroom.ClassroomId,
                Name = classroom.Name,
                CourseName = classroom.Course.Name,
                ProgramName = classroom.Course.AcademicProgram.Name,
                EnrolledStudentsCount = classroom.Enrollments.Count,
                Capacity = classroom.Capacity,
                LectureCount = classroom.Lectures.Count,
                Status = classroom.Status.ToString(),
                Announcements = announcements.Select(a => new SimpleAnnouncementDto
                {
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Content = a.Content,
                    PostedAt = a.PostedAt
                }).ToList()
            };

            return Ok(classroomDetailsDto);
        }

        [HttpPost("classrooms/{classroomId}/toggle-status")]
        public async Task<IActionResult> ToggleClassroomStatus(int classroomId)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized(new { message = "معرّف المستخدم غير صالح." });

            var classroom = await _classroomRepository.GetClassroomByIdAsync(classroomId);
            if (classroom == null) return NotFound(new { message = $"لم يتم العثور على فصل بالمعرّف {classroomId}." });
            if (classroom.TeacherId != teacherId) return Forbid();

            string successMessage;
            if (classroom.Status == ClassroomStatus.ACTIVE)
            {
                var enrollments = (await _enrollmentRepository.GetEnrollmentsForClassroomAsync(classroomId)).ToList();
                if (enrollments.Any() && !enrollments.All(e => e.FinalGrade.HasValue))
                {
                    return BadRequest(new { message = "لا يمكن إكمال الفصل. لم يتم رصد الدرجات النهائية لجميع الطلاب." });
                }
                classroom.Status = ClassroomStatus.COMPLETED;
                successMessage = "تم بنجاح تحديد الفصل كمكتمل.";
            }
            else if (classroom.Status == ClassroomStatus.COMPLETED)
            {
                classroom.Status = ClassroomStatus.ACTIVE;
                successMessage = "تم بنجاح إعادة تفعيل الفصل.";
            }
            else
            {
                return BadRequest(new { message = $"لا يمكن تغيير حالة الفصل وهو في الحالة '{classroom.Status}'." });
            }

            await _classroomRepository.SaveChangesAsync();
            return Ok(new { message = successMessage });
        }

        #endregion

        #region إدارة المحاضرات والمحتوى

        [HttpPost("classrooms/{classroomId}/lectures")]
        public async Task<IActionResult> CreateLecture(int classroomId, [FromBody] CreateLectureDto createDto)
        {
            var teacherIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(teacherIdString, out var teacherId))
            {
                return Unauthorized("Invalid user identifier in token.");
            }

            var classroom = await _classroomRepository.GetClassroomByIdAsync(classroomId);
            if (classroom == null)
            {
                return NotFound("Classroom not found.");
            }
            if (classroom.TeacherId != teacherId)
            {
                return Forbid();
            }

            if (classroom.Status != ClassroomStatus.ACTIVE)
            {
                return BadRequest("You cannot add lectures to a non-active classroom.");
            }

            var lecture = new Lecture
            {
                Title = createDto.Title,
                Description = createDto.Description,
                LectureOrder = createDto.LectureOrder,
                ClassroomId = classroomId,
                CreatedAt = DateTime.UtcNow
            };

            _lectureRepository.CreateLecture(lecture);
            await _lectureRepository.SaveChangesAsync();

            var lectureDto = new LectureDto
            {
                LectureId = lecture.LectureId,
                Title = lecture.Title,
                Description = lecture.Description,
                LectureOrder = lecture.LectureOrder,
                CreatedAt = lecture.CreatedAt
            };

            return CreatedAtAction(nameof(GetClassroomLectures), new { classroomId = classroomId }, lectureDto);
        }

        [HttpGet("classrooms/{classroomId}/lectures")]
        public async Task<ActionResult<IEnumerable<LectureContentDto>>> GetClassroomLectures(int classroomId)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();

            var classroom = await _classroomRepository.GetClassroomByIdAsync(classroomId);
            if (classroom == null) return NotFound(new { message = "لم يتم العثور على الفصل." });
            if (classroom.TeacherId != teacherId) return Forbid();

            var lectures = await _lectureRepository.GetLecturesWithMaterialsAsync(classroomId);

            var lectureDtos = lectures.Select(l => new LectureContentDto
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
                    Description = m.Description,
                    MaterialType = m.MaterialType,
                    Url = m.MaterialType == "Link" ? m.Url : null,
                    OriginalFilename = m.OriginalFilename,
                    FileSize = m.FileSize,
                    UploadedAt = m.UploadedAt
                }).ToList(),
                LectureQuiz = l.LectureQuiz != null ? new LectureQuizSummaryDto
                {
                    LectureQuizId = l.LectureQuiz.LectureQuizId,
                    Title = l.LectureQuiz.Title
                } : null
            }).ToList();

            return Ok(lectureDtos);
        }

        [HttpDelete("lectures/{lectureId}")]
        public async Task<IActionResult> DeleteLecture(int lectureId)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();

            var lectureFromRepo = await _lectureRepository.GetLectureByIdAsync(lectureId);
            if (lectureFromRepo == null) return NoContent();

            var classroom = await _classroomRepository.GetClassroomByIdAsync(lectureFromRepo.ClassroomId);
            if (classroom == null || classroom.TeacherId != teacherId) return Forbid();
            if (classroom.Status != ClassroomStatus.ACTIVE) return BadRequest(new { message = "لا يمكنك حذف محاضرات من فصل غير نشط." });

            _lectureRepository.DeleteLecture(lectureFromRepo);
            await _lectureRepository.SaveChangesAsync();

            return NoContent();
        }

        #endregion

        #region إدارة المواد التعليمية

        [HttpGet("materials/{materialId}", Name = "GetMaterialById")]
        public async Task<IActionResult> GetMaterialById(int materialId)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized(new { message = "معرّف المستخدم غير صالح." });

            var material = await _materialRepository.GetMaterialByIdAsync(materialId);
            if (material == null) return NotFound(new { message = $"لم يتم العثور على مادة بالمعرّف {materialId}." });
            if (material.Lecture == null) return Forbid();

            var classroom = await _classroomRepository.GetClassroomByIdAsync(material.Lecture.ClassroomId);
            if (classroom == null || classroom.TeacherId != teacherId) return Forbid();

            var materialDto = new MaterialDto
            {
                MaterialId = material.MaterialId,
                Title = material.Title,
                Description = material.Description,
                MaterialType = material.MaterialType,
                Url = material.MaterialType == "Link" ? material.Url : null,
                OriginalFilename = material.OriginalFilename,
                FileSize = material.FileSize,
                UploadedAt = material.UploadedAt
            };

            return Ok(materialDto);
        }

        [HttpGet("from-course/{courseId}/material")]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetCourseMaterials(int courseId)
        {
            if (await _courseRepository.GetCourseByIdAsync(courseId) == null)
                return NotFound(new { message = "لم يتم العثور على الدورة." });

            var materials = await _materialRepository.GetMaterialsForCourseAsync(courseId);
            var materialsDto = materials.Select(m => new MaterialDto
            {
                MaterialId = m.MaterialId,
                Title = m.Title,
                Description = m.Description,
                MaterialType = m.MaterialType,
                Url = m.MaterialType == "Link" ? m.Url : null,
                OriginalFilename = m.OriginalFilename,
                FileSize = m.FileSize,
                UploadedAt = m.UploadedAt
            });

            return Ok(materialsDto);
        }

        [HttpPost("lectures/{lectureId}/materials")]
        public async Task<IActionResult> AddMaterialToLecture(int lectureId, [FromForm] CreateMaterialDto materialDto)
        {
            bool isFileProvided = materialDto.File != null && materialDto.File.Length > 0;
            bool isUrlProvided = !string.IsNullOrWhiteSpace(materialDto.Url);
            if (!isFileProvided && !isUrlProvided) return BadRequest(new { message = "يجب عليك توفير ملف أو رابط." });
            if (isFileProvided && isUrlProvided) return BadRequest(new { message = "لا يمكنك توفير ملف ورابط في نفس الوقت." });

            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();
            var lecture = await _lectureRepository.GetLectureByIdAsync(lectureId);
            if (lecture == null) return NotFound(new { message = "لم يتم العثور على المحاضرة." });
            var classroom = await _classroomRepository.GetClassroomByIdAsync(lecture.ClassroomId);
            if (classroom == null || classroom.TeacherId != teacherId) return Forbid();
            if (classroom.Status != ClassroomStatus.ACTIVE) return BadRequest(new { message = "لا يمكنك إضافة مواد إلى محاضرة في فصل غير نشط." });

            var material = new Material
            {
                Title = materialDto.Title,
                Description = materialDto.Description,
                UploadedAt = DateTime.UtcNow,
                LectureId = lectureId
            };

            if (isFileProvided)
            {
                var uploadResult = await _fileService.SaveFileAsync(materialDto.File, "lecture_materials");
                material.Url = uploadResult.Url;
                material.PublicId = uploadResult.PublicId;
                material.MaterialType = "File";
                material.OriginalFilename = materialDto.File.FileName;
                material.FileSize = materialDto.File.Length;
            }
            else
            {
                material.Url = materialDto.Url;
                material.MaterialType = "Link";
            }

            await _materialRepository.CreateMaterialAsync(material);
            await _materialRepository.SaveChangesAsync();

            var materialDtoToReturn = new MaterialDto
            {
                MaterialId = material.MaterialId,
                Title = material.Title,
                Description = material.Description,
                MaterialType = material.MaterialType,
                OriginalFilename = material.OriginalFilename,
                FileSize = material.FileSize,
                UploadedAt = material.UploadedAt
            };

            return CreatedAtAction(nameof(GetMaterialById), new { materialId = material.MaterialId }, materialDtoToReturn);
        }

        [HttpDelete("materials/{materialId}")]
        public async Task<IActionResult> DeleteMaterial(int materialId)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized(new { message = "معرّف المستخدم غير صالح." });

            var materialFromRepo = await _materialRepository.GetMaterialByIdAsync(materialId);
            if (materialFromRepo == null) return NoContent();
            if (materialFromRepo.Lecture == null) return Forbid();

            var classroom = await _classroomRepository.GetClassroomByIdAsync(materialFromRepo.Lecture.ClassroomId);
            if (classroom == null || classroom.TeacherId != teacherId) return Forbid();
            if (classroom.Status != ClassroomStatus.ACTIVE) return BadRequest(new { message = "لا يمكنك حذف مواد من فصل غير نشط." });

            if (materialFromRepo.MaterialType == "File" && !string.IsNullOrEmpty(materialFromRepo.PublicId))
            {
                await _fileService.DeleteFileAsync(materialFromRepo.PublicId);
            }

            _materialRepository.DeleteMaterial(materialFromRepo);
            await _materialRepository.SaveChangesAsync();

            return NoContent();
        }

        //[HttpGet("materials/{materialId}/download")]
        //public async Task<IActionResult> DownloadMaterial(int materialId)
        //{
        //    var teacherId = GetCurrentUserId();
        //    if (teacherId == null) return Unauthorized(new { message = "معرّف المستخدم غير صالح." });

        //    var material = await _materialRepository.GetMaterialWithDeepDetailsAsync(materialId);
        //    if (material == null || material.MaterialType != "File" || string.IsNullOrEmpty(material.Url))
        //    {
        //        return NotFound(new { message = "الملف غير موجود أو غير صالح للتحميل." });
        //    }

        //    bool hasAccess = false;
        //    if (material.Lecture?.Classroom != null)
        //    {
        //        if (material.Lecture.Classroom.TeacherId == teacherId)
        //        {
        //            hasAccess = true;
        //        }
        //    }
        //    else if (material.Course != null)
        //    {
        //        bool teachesInThisCourse = (await _classroomRepository.GetClassroomsByCourseAsync(material.Course.CourseId))
        //                                   .Any(c => c.TeacherId == teacherId);
        //        if (teachesInThisCourse)
        //        {
        //            hasAccess = true;
        //        }
        //    }

        //    if (!hasAccess)
        //    {
        //        return Forbid();
        //    }

        //    return Redirect(material.Url);
        //}
        #endregion

        #region إدارة الطلاب والدرجات

        [HttpGet("classrooms/{classroomId}/enrollments")]
        public async Task<IActionResult> GetClassroomEnrollments(int classroomId)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized(new { message = "معرّف المستخدم غير صالح." });

            var classroom = await _classroomRepository.GetClassroomByIdAsync(classroomId);
            if (classroom == null) return NotFound(new { message = $"لم يتم العثور على فصل بالمعرّف {classroomId}." });
            if (classroom.TeacherId != teacherId) return Forbid();

            var enrollments = await _enrollmentRepository.GetEnrollmentsForClassroomAsync(classroomId);
            var enrollmentDtos = enrollments.Select(e => new EnrollmentDto
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                StudentName = $"{e.Student.FirstName} {e.Student.LastName}",
                ClassroomId = e.ClassroomId,
                ClassroomName = e.Classroom.Name,
                EnrollmentDate = e.EnrollmentDate,
                PracticalGrade = e.PracticalGrade,
                ExamGrade = e.ExamGrade,
                FinalGrade = e.FinalGrade
            }).ToList();

            return Ok(enrollmentDtos);
        }

        [HttpPost("enrollments/{enrollmentId}/raw-grades")]
        public async Task<IActionResult> SetRawGrades(int enrollmentId, [FromBody] SetRawGradesDto gradesDto)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized(new { message = "معرّف المستخدم غير صالح." });

            var enrollment = await _enrollmentRepository.GetEnrollmentWithDetailsByIdAsync(enrollmentId);
            if (enrollment == null) return NotFound(new { message = $"لم يتم العثور على سجل تسجيل بالمعرّف {enrollmentId}." });
            if (enrollment.Classroom == null || enrollment.Classroom.TeacherId != teacherId) return Forbid();
            if (enrollment.Classroom.Status != ClassroomStatus.ACTIVE) return BadRequest(new { message = "لا يمكنك رصد الدرجات لفصل غير نشط." });

            enrollment.PracticalGrade = gradesDto.PracticalGrade;
            enrollment.ExamGrade = gradesDto.ExamGrade;

            await _enrollmentRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("classrooms/{classroomId}/grading-status")]
        public async Task<IActionResult> GetGradingStatus(int classroomId)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized(new { message = "معرّف المستخدم غير صالح." });

            var classroom = await _classroomRepository.GetClassroomByIdAsync(classroomId);
            if (classroom == null) return NotFound(new { message = $"لم يتم العثور على فصل بالمعرّف {classroomId}." });
            if (classroom.TeacherId != teacherId) return Forbid();

            var enrollments = await _enrollmentRepository.GetEnrollmentsForClassroomAsync(classroomId);

            var totalStudents = enrollments.Count();
            var gradesEnteredCount = 0;
            var missingGradesStudents = new List<StudentInfoForReportDto>();

            foreach (var enrollment in enrollments)
            {
                if (enrollment.PracticalGrade.HasValue && enrollment.ExamGrade.HasValue)
                {
                    gradesEnteredCount++;
                }
                else
                {
                    missingGradesStudents.Add(new StudentInfoForReportDto
                    {
                        StudentId = enrollment.StudentId,
                        StudentName = $"{enrollment.Student.FirstName} {enrollment.Student.LastName}"
                    });
                }
            }

            var report = new GradingStatusReportDto
            {
                TotalStudents = totalStudents,
                GradesEnteredCount = gradesEnteredCount,
                GradesMissingCount = totalStudents - gradesEnteredCount,
                IsComplete = totalStudents > 0 && gradesEnteredCount == totalStudents,
                MissingGradesForStudents = missingGradesStudents
            };

            return Ok(report);
        }

        [HttpPost("classrooms/{classroomId}/calculate-final-grades")]
        public async Task<IActionResult> CalculateFinalGrades(int classroomId)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized(new { message = "معرّف المستخدم غير صالح." });

            var classroom = await _classroomRepository.GetClassroomByIdAsync(classroomId);
            if (classroom == null) return NotFound(new { message = $"لم يتم العثور على فصل بالمعرّف {classroomId}." });
            if (classroom.TeacherId != teacherId) return Forbid();
            if (classroom.Status != ClassroomStatus.ACTIVE) return BadRequest(new { message = "لا يمكن حساب الدرجات النهائية إلا لفصل نشط." });

            var enrollments = (await _enrollmentRepository.GetEnrollmentsForClassroomAsync(classroomId)).ToList();
            if (!enrollments.Any()) return BadRequest(new { message = "لا يوجد طلاب في هذا الفصل لحساب درجاتهم." });

            if (!enrollments.All(e => e.PracticalGrade.HasValue && e.ExamGrade.HasValue))
            {
                return BadRequest(new { message = "لا يمكن حساب الدرجات النهائية. لم يتم رصد درجات العملي والامتحان لجميع الطلاب." });
            }

            foreach (var enrollment in enrollments)
            {
                enrollment.FinalGrade = (enrollment.PracticalGrade + enrollment.ExamGrade) / 2;
            }

            await _enrollmentRepository.SaveChangesAsync();
            return Ok(new { message = "تم حساب وحفظ الدرجات النهائية بنجاح لجميع الطلاب." });
        }

        #endregion

        #region التواصل

        [HttpPost("classrooms/{classroomId}/announcements")]
        public async Task<IActionResult> CreateClassroomAnnouncement(int classroomId, [FromBody] CreateAnnouncementDto announcementDto)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized(new { message = "معرّف المستخدم غير صالح." });

            var classroom = await _classroomRepository.GetClassroomByIdAsync(classroomId);
            if (classroom == null) return NotFound(new { message = $"لم يتم العثور على فصل بالمعرّف {classroomId}." });
            if (classroom.TeacherId != teacherId) return Forbid();

            var announcement = new Announcement
            {
                Title = announcementDto.Title,
                Content = announcementDto.Content,
                PostedAt = DateTime.UtcNow,
                CreatedByUserId = teacherId,
                TargetScope = AnnouncementScope.CLASSROOM,
                ClassroomId = classroomId,
                AcademicProgramId = null
            };

            await _announcementRepository.CreateAnnouncementAsync(announcement);
            await _announcementRepository.SaveChangesAsync();

            var announcementToReturn = new AnnouncementDto
            {
                AnnouncementId = announcement.AnnouncementId,
                Title = announcement.Title,
                Content = announcement.Content,
                PostedAt = announcement.PostedAt,
                TargetScope = announcement.TargetScope,
                TargetId = announcement.ClassroomId,
                TargetName = classroom.Name
            };

            return Ok(announcementToReturn);
        }

        #endregion

        #region التحدي الأسبوعي (Weekly Challenge)

        [HttpGet("challenge/course/{courseId}/leaderboard")]
        public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetLeaderboardForTeacher(int courseId)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == null) return Unauthorized();

            var course = await _courseRepository.GetCourseByIdAsync(courseId);
            if (course == null) return NotFound(new { message = "المساق غير موجود." });

            bool isAssociated = await _classroomRepository.IsTeacherAssociatedWithCourseAsync(teacherId.Value, courseId);
            if (!isAssociated && course.CoordinatorId != teacherId)
            {
                return Forbid();
            }

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

        #endregion

        #region الدوال المساعدة الخاصة
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }

        private (int Year, int Week) GetCurrentWeekNumber()
        {
            var now = System.DateTime.UtcNow;
            var calendar = CultureInfo.InvariantCulture.Calendar;
            var weekOfYear = calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return (now.Year, weekOfYear);
        }
        #endregion
    }
}