using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Repositories
{
    public class GraduationRepository : IGraduationRepository
    {
        private readonly SmartSchoolDbContext _context;

        public GraduationRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        #region عمليات قراءة سجلات الخريجين

        public async Task<Graduation?> GetGraduationByIdAsync(int graduationId)
        {
             return await _context.Graduations
                .Include(g => g.Certificate)
                .FirstOrDefaultAsync(g => g.GraduationId == graduationId);
        }

        public async Task<Graduation?> GetGraduationByStudentIdAsync(int studentId)
        {
            return await _context.Graduations
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.StudentUserId == studentId);
        }

        public async Task<IEnumerable<Graduation>> GetGraduationsByNationalIdAsync(string nationalId)
        {
            return await _context.Graduations
                .Include(g => g.Certificate)
                .Where(g => g.NationalId == nationalId)
                .AsNoTracking()
                .OrderByDescending(g => g.GraduationDate)
                .ToListAsync();
        }

        public async Task<Graduation?> GetGraduationRecordAsync(int studentId, int programId)
        {
            return await _context.Graduations
                .Include(g => g.Certificate)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.StudentUserId == studentId && g.AcademicProgramId == programId);
        }

        public async Task<IEnumerable<Graduation>> GetGraduatesAsync(int? programId, int? year, int? month)
        {
            var query = _context.Graduations.Include(g => g.Certificate).AsQueryable();

            if (programId.HasValue) query = query.Where(g => g.AcademicProgramId == programId.Value);
            if (year.HasValue) query = query.Where(g => g.GraduationDate.Year == year.Value);
            if (month.HasValue) query = query.Where(g => g.GraduationDate.Month == month.Value);

            return await query.AsNoTracking().OrderByDescending(g => g.GraduationDate).ToListAsync();
        }

        public async Task<IEnumerable<Graduation>> GetGraduatesPendingCertificateAsync()
        {
            return await _context.Graduations
                .Where(g => g.Certificate == null)
                .AsNoTracking()
                .OrderBy(g => g.GraduationDate)
                .ToListAsync();
        }

        #endregion

        #region عمليات قراءة سجلات الراسبين

        public async Task<FailedStudent?> GetFailureByStudentIdAsync(int studentId)
        {
            return await _context.FailedStudents
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.StudentUserId == studentId);
        }

        public async Task<IEnumerable<FailedStudent>> GetFailuresAsync(int? programId, int? year, int? month)
        {
            var query = _context.FailedStudents.AsQueryable();

            if (programId.HasValue) query = query.Where(f => f.AcademicProgramId == programId.Value);
            if (year.HasValue) query = query.Where(f => f.FailureDate.Year == year.Value);
            if (month.HasValue) query = query.Where(f => f.FailureDate.Month == month.Value);

            return await query.AsNoTracking().OrderByDescending(f => f.FailureDate).ToListAsync();
        }

        #endregion

        #region عمليات التحقق

        public async Task<bool> HasAlreadyGraduatedAsync(int studentId, int programId)
        {
            return await _context.Graduations.AnyAsync(g => g.StudentUserId == studentId && g.AcademicProgramId == programId);
        }

        public async Task<bool> HasFailedAsync(int studentId, int programId)
        {
            return await _context.FailedStudents.AnyAsync(f => f.StudentUserId == studentId && f.AcademicProgramId == programId);
        }

        #endregion

        #region عمليات الكتابة (الإنشاء والحذف)

        public async Task CreateGraduationRecordAsync(Graduation graduation)
        {
            await _context.Graduations.AddAsync(graduation);
        }

        public async Task CreateFailedStudentRecordAsync(FailedStudent failedStudent)
        {
            await _context.FailedStudents.AddAsync(failedStudent);
        }

        public async Task AddCertificateAsync(GraduationCertificate certificate)
        {
            await _context.GraduationCertificates.AddAsync(certificate);
        }

        public async Task DeleteCertificateAsync(int graduationId)
        {
             var certificate = await _context.GraduationCertificates.FirstOrDefaultAsync(c => c.GraduationId == graduationId);
            if (certificate != null)
            {
                _context.GraduationCertificates.Remove(certificate);
            }
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