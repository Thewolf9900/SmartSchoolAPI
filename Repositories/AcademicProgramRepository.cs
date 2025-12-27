using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Repositories
{
    public class AcademicProgramRepository : IAcademicProgramRepository
    {
        private readonly SmartSchoolDbContext _context;

        public AcademicProgramRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        #region عمليات القراءة (Read Operations)

        public async Task<AcademicProgram?> GetProgramByIdAsync(int programId)
        {
             return await _context.AcademicPrograms.FindAsync(programId);
        }

        public async Task<IEnumerable<AcademicProgram>> GetAllProgramsAsync()
        {
             return await _context.AcademicPrograms.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<AcademicProgram>> GetProgramsOpenForRegistrationAsync()
        {
            return await _context.AcademicPrograms
                .Include(p => p.Courses)
                .Where(p => p.IsRegistrationOpen)
                .ToListAsync();
        }


        #endregion

        #region عمليات الكتابة (Write Operations)

        public async Task CreateProgramAsync(AcademicProgram program)
        {
            await _context.AcademicPrograms.AddAsync(program);
        }

        public void UpdateProgram(AcademicProgram program)
        {
            // لا حاجة لأي كود هنا. EF Core Change Tracker يتولى الأمر.
        }

        public void DeleteProgram(AcademicProgram program)
        {
            _context.AcademicPrograms.Remove(program);
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