using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Repositories
{
    public class ClassroomRepository : IClassroomRepository
    {
        private readonly SmartSchoolDbContext _context;

        public ClassroomRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        #region دوال عامة (تستخدم من قبل أدوار متعددة)

        public async Task<Classroom?> GetClassroomByIdAsync(int classroomId)
        {
            // هذا الاستعلام يتطلب التتبع لأنه قد يتبعه تعديل (مثل تحديث بيانات الفصل)
            return await _context.Classrooms
               .Include(c => c.Teacher)
               .Include(c => c.Course)
                   .ThenInclude(course => course.AcademicProgram)
               .Include(c => c.Enrollments)
               .FirstOrDefaultAsync(c => c.ClassroomId == classroomId);
        }

        #endregion

        #region دوال خاصة بالمدير (Admin)

        public async Task<IEnumerable<Classroom>> GetAllClassroomsAsync(ClassroomStatus? status)
        {
            var query = _context.Classrooms
                .Include(c => c.Course)
                    .ThenInclude(co => co.AcademicProgram)
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Classroom>> GetClassroomsByCourseAsync(int courseId)
        {
            return await _context.Classrooms
                .Where(c => c.CourseId == courseId)
                .Include(c => c.Teacher)
                .Include(c => c.Course)
                    .ThenInclude(course => course.AcademicProgram)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Classroom>> GetClassroomsWithoutTeacherAsync()
        {
            return await _context.Classrooms
                .Where(c => c.TeacherId == null)
                .Include(c => c.Course)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task CreateClassroomAsync(Classroom classroom)
        {
            await _context.Classrooms.AddAsync(classroom);
        }

        public void UpdateClassroom(Classroom classroom) { }

        public void DeleteClassroom(Classroom classroom)
        {
            _context.Classrooms.Remove(classroom);
        }

        #endregion

        #region دوال خاصة بالمدرس (Teacher)

        public async Task<IEnumerable<Classroom>> GetClassroomsByTeacherIdAsync(int teacherId, ClassroomStatus? status)
        {
            var query = _context.Classrooms
                                 .Include(c => c.Course)
                                    .ThenInclude(course => course.AcademicProgram)
                                 .Include(c => c.Enrollments)
                                 .Include(c => c.Teacher)
                                 .Where(c => c.TeacherId == teacherId);

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return await query.AsNoTracking().OrderBy(c => c.Course.Name).ToListAsync();
        }

        public async Task<Classroom?> GetClassroomWithDetailsForTeacherPortalAsync(int classroomId)
        {
            return await _context.Classrooms
                .Include(c => c.Course)
                    .ThenInclude(course => course.AcademicProgram)
                .Include(c => c.Enrollments)
                .Include(c => c.Lectures)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClassroomId == classroomId);
        }

        public async Task<IEnumerable<Classroom>> GetClassroomsByTeacherAsync(int teacherId)
        {
            return await _context.Classrooms
                .Where(c => c.TeacherId == teacherId)
                .Include(c => c.Course)
                    .ThenInclude(course => course.AcademicProgram)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region دوال خاصة بالطالب (Student)

        public async Task<Classroom?> GetClassroomDetailsForStudentAsync(int classroomId)
        {
            return await _context.Classrooms
              .Include(c => c.Course)
        .Include(c => c.Teacher)
        .Include(c => c.Enrollments)
        .Include(c => c.Lectures)
            .ThenInclude(l => l.Materials)
        .Include(c => c.Lectures) 
            .ThenInclude(l => l.LectureQuiz)  
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.ClassroomId == classroomId);
        }

        public async Task<bool> IsTeacherAssociatedWithCourseAsync(int teacherId, int courseId)
        {
            // تتحقق مما إذا كان المدرس معينًا على الأقل لفصل واحد ضمن هذه الدورة
            return await _context.Classrooms
                .AnyAsync(c => c.CourseId == courseId && c.TeacherId == teacherId);
        }
        #endregion

        #region إدارة حفظ البيانات

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        #endregion
    }
}