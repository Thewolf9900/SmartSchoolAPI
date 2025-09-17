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
    public class CourseRepository : ICourseRepository
    {
        private readonly SmartSchoolDbContext _context;

        public CourseRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        #region عمليات القراءة

        public async Task<Course?> GetCourseByIdAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.AcademicProgram)
                .Include(c => c.Coordinator)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.AcademicProgram)
                .Include(c => c.Coordinator)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByProgramAsync(int academicProgramId)
        {
            return await _context.Courses
                .Where(c => c.AcademicProgramId == academicProgramId)
                .Include(c => c.AcademicProgram)
                 .Include(c => c.Coordinator)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesWithoutClassroomsAsync()
        {
            return await _context.Courses
                .Where(c => !c.Classrooms.Any())
                .Include(c => c.AcademicProgram)
                 .Include(c => c.Coordinator)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesWithoutCoordinatorAsync()
        {
            return await _context.Courses
                .Where(c => c.CoordinatorId == null) // الشرط الأساسي للتقرير
                .Include(c => c.AcademicProgram)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IEnumerable<Course>> GetCoursesByCoordinatorIdAsync(int coordinatorId)
        {
            return await _context.Courses
                .Where(c => c.CoordinatorId == coordinatorId)
                .Include(c => c.AcademicProgram)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region عمليات الكتابة

        public async Task CreateCourseAsync(Course course)
        {
            await _context.Courses.AddAsync(course);
        }

        public void UpdateCourse(Course course)
        {
            // EF Core Change Tracker يتولى الأمر
        }

        public void DeleteCourse(Course course)
        {
            _context.Courses.Remove(course);
        }


        public async Task<(bool Success, string ErrorMessage)> AssignCoordinatorAsync(int courseId, int teacherId)
        {
            // 1. البحث عن الدورة
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return (false, "الدورة غير موجودة.");
            }

            // 2. البحث عن المستخدم (المدرس)
            var teacher = await _context.Users.FindAsync(teacherId);
            if (teacher == null)
            {
                return (false, "المدرس المحدد غير موجود.");
            }

            // 3. التحقق من أن المستخدم هو مدرس بالفعل
            if (teacher.Role != UserRole.Teacher)
            {
                return (false, "المستخدم المحدد ليس مدرسًا، لا يمكن تعيينه كمسؤول.");
            }

            // 4. تنفيذ عملية التعيين
            course.CoordinatorId = teacherId;

            return (true, string.Empty); 
        }
        public async Task<bool> UnassignCoordinatorAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return false; // الدورة غير موجودة
            }

            course.CoordinatorId = null; // إلغاء التعيين
            return true;
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