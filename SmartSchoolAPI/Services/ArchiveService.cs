using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Services
{
    public class ArchiveService : IArchiveService
    {
        private readonly IGraduationRepository _graduationRepo;
        private readonly IArchiveRepository _archiveRepo;
        private readonly SmartSchoolDbContext _context;

        public ArchiveService(IGraduationRepository graduationRepo,
            IArchiveRepository archiveRepo, SmartSchoolDbContext context)
        {
            _graduationRepo = graduationRepo;
            _archiveRepo = archiveRepo;
            _context = context;
        }

        public async Task<(bool Success, string Message)> ArchiveClassroomAsync(int classroomId)
        {
            var classroomToArchive = await _context.Classrooms
                .Include(c => c.Course).ThenInclude(co => co.AcademicProgram)
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments).ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.ClassroomId == classroomId);

            if (classroomToArchive == null)
            {
                return (false, "لم يتم العثور على الفصل الدراسي المحدد.");
            }

            if (classroomToArchive.Status != ClassroomStatus.COMPLETED)
            {
                return (false, "لا يمكن أرشفة الفصل إلا إذا كانت حالته 'مكتمل'.");
            }

            var programId = classroomToArchive.Course.AcademicProgramId;
            foreach (var enrollment in classroomToArchive.Enrollments)
            {
                var student = enrollment.Student;
                if (student == null) continue;

                bool isProcessed = await _graduationRepo.HasAlreadyGraduatedAsync(student.UserId, programId) ||
                                   await _graduationRepo.HasFailedAsync(student.UserId, programId);

                if (!isProcessed)
                {
                    return (false, $"لا يمكن أرشفة الفصل. الطالب '{student.FirstName} {student.LastName}' لم تتم معالجة نتيجته النهائية بعد.");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var archivedClassroom = new ArchivedClassroom
                {
                    OriginalClassroomId = classroomToArchive.ClassroomId,
                    Name = classroomToArchive.Name,
                    CourseName = classroomToArchive.Course.Name,
                    ProgramName = classroomToArchive.Course.AcademicProgram.Name,
                    TeacherName = classroomToArchive.Teacher != null ? $"{classroomToArchive.Teacher.FirstName} {classroomToArchive.Teacher.LastName}" : null,
                    ArchivedAt = DateTime.UtcNow
                };

                foreach (var enrollment in classroomToArchive.Enrollments)
                {
                    archivedClassroom.ArchivedEnrollments.Add(new ArchivedEnrollment
                    {
                        StudentName = $"{enrollment.Student.FirstName} {enrollment.Student.LastName}",
                        StudentNationalId = enrollment.Student.NationalId,
                        PracticalGrade = enrollment.PracticalGrade,
                        ExamGrade = enrollment.ExamGrade,
                        FinalGrade = enrollment.FinalGrade
                    });
                }

                await _archiveRepo.CreateArchivedClassroomAsync(archivedClassroom);
                await _archiveRepo.SaveChangesAsync();

                 var studentsInArchivedClassroom = classroomToArchive.Enrollments.Select(e => e.Student).ToList();

                // أولاً، احذف الفصل نفسه. هذا سيحذف كل التسجيلات المرتبطة به.
                _context.Classrooms.Remove(classroomToArchive);

                // الآن، تحقق من كل طالب كان في الفصل المحذوف
                foreach (var student in studentsInArchivedClassroom)
                {
                    if (student == null) continue;

                    // هل تمت معالجة هذا الطالب كخريج أو راسب؟
                    bool isProcessed = await _graduationRepo.HasAlreadyGraduatedAsync(student.UserId, programId) ||
                                       await _graduationRepo.HasFailedAsync(student.UserId, programId);


                    // الشرط الجديد: احذف الطالب فقط إذا تمت معالجته **وليس** لديه برنامج نشط حاليًا
                    if (isProcessed)
                    {
                        _context.Users.Remove(student);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "تمت أرشفة الفصل الدراسي بنجاح.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return (false, "حدث خطأ غير متوقع أثناء عملية الأرشفة.");
            }
        }
    }
}