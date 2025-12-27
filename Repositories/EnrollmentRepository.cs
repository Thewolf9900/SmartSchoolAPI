using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Repositories
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly SmartSchoolDbContext _context;

        public EnrollmentRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        #region عمليات القراءة

        public async Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId)
        {
             return await _context.Enrollments.FindAsync(enrollmentId);
        }

        public async Task<Enrollment?> GetEnrollmentWithDetailsByIdAsync(int enrollmentId)
        {
             return await _context.Enrollments
               .Include(e => e.Classroom)
               .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsForClassroomAsync(int classroomId)
        {
            return await _context.Enrollments
                .Where(e => e.ClassroomId == classroomId)
                .Include(e => e.Student)
                .Include(e => e.Classroom)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsForStudentAsync(int studentId)
        {
            return await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Course)
                .Include(e => e.Classroom)
                    .ThenInclude(c => c.Teacher)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> IsStudentEnrolledAsync(int studentId, int classroomId)
        {
            return await _context.Enrollments
                .AsNoTracking()
                .AnyAsync(e => e.StudentId == studentId && e.ClassroomId == classroomId);
        }


        public async Task<bool> IsStudentEnrolledInCourseAsync(int studentId, int courseId)
        {
             
            return await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.Classroom.CourseId == courseId);
        }
        #endregion

        #region عمليات الكتابة

        public async Task CreateEnrollmentAsync(Enrollment enrollment)
        {
            await _context.Enrollments.AddAsync(enrollment);
        }

        public void DeleteEnrollment(Enrollment enrollment)
        {
            _context.Enrollments.Remove(enrollment);
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