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
    public class UserRepository : IUserRepository
    {
        private readonly SmartSchoolDbContext _context;

        public UserRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        #region عمليات القراءة العامة

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            // قد يسبق عملية تعديل، لذا لا نستخدم AsNoTracking
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByNationalIdAsync(string nationalId)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.NationalId == nationalId);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(UserRole? role)
        {
            var query = _context.Users.AsQueryable();

            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<User?> GetUserWithProgramByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.AcademicProgram)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        #endregion

        #region عمليات القراءة الخاصة بالطلاب

        public async Task<User?> GetStudentProfileByIdAsync(int studentId)
        {
            return await _context.Users
                .Where(u => u.UserId == studentId && u.Role == UserRole.Student)
                .Include(u => u.AcademicProgram)
                .Include(u => u.Enrollments)
                    .ThenInclude(e => e.Classroom)
                        .ThenInclude(c => c.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetActiveStudentsAsync()
        {
            var processedStudentIds = _context.Graduations.Select(g => g.StudentUserId)
                .Concat(_context.FailedStudents.Select(f => f.StudentUserId));

            return await _context.Users
                .Where(u => u.Role == UserRole.Student &&
                            u.AcademicProgramId != null &&
                            !processedStudentIds.Contains(u.UserId))
                .OrderBy(u => u.FirstName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUnassignedStudentsAsync()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Student && u.AcademicProgramId == null)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetStudentsByProgramAsync(int programId)
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Student && u.AcademicProgramId == programId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAvailableStudentsForCourseAsync(int courseId)
        {
            var programId = await _context.Courses
                .Where(c => c.CourseId == courseId)
                .Select(c => c.AcademicProgramId)
                .FirstOrDefaultAsync();

            if (programId == default)
            {
                return Enumerable.Empty<User>();
            }

            var enrolledStudentIds = _context.Enrollments
                .Where(e => e.Classroom.CourseId == courseId)
                .Select(e => e.StudentId);

            return await _context.Users
                .Where(u => u.Role == UserRole.Student &&
                            u.AcademicProgramId == programId &&
                            !enrolledStudentIds.Contains(u.UserId))
                .OrderBy(u => u.LastName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetStudentsWithEnrollmentsByProgramAsync(int programId)
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Student && u.AcademicProgramId == programId)
                .Include(u => u.Enrollments)
                    .ThenInclude(e => e.Classroom)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region عمليات القراءة الخاصة بالمدرسين

        public async Task<IEnumerable<User>> GetTeachersByCourseAsync(int courseId)
        {
            var teacherIdsInCourse = _context.Classrooms
                .Where(c => c.CourseId == courseId && c.TeacherId != null)
                .Select(c => c.TeacherId);

            return await _context.Users
                .Where(u => teacherIdsInCourse.Contains(u.UserId))
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region عمليات الكتابة وإدارة الحساب

        public async Task CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public void DeleteUser(User user)
        {
            _context.Users.Remove(user);
        }

        public async Task<PasswordResetToken?> GetPasswordResetTokenByUserIdAsync(int userId)
        {
            return await _context.PasswordResetTokens.FindAsync(userId);
        }

        public async Task SetPasswordResetTokenAsync(PasswordResetToken token)
        {
            var existingToken = await _context.PasswordResetTokens.FindAsync(token.UserId);
            if (existingToken != null)
            {
                _context.PasswordResetTokens.Remove(existingToken);
            }
            await _context.PasswordResetTokens.AddAsync(token);
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