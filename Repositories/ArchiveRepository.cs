using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Repositories
{
    public class ArchiveRepository : IArchiveRepository
    {
        private readonly SmartSchoolDbContext _context;

        public ArchiveRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        #region عمليات القراءة

        public async Task<IEnumerable<ArchivedClassroom>> GetAllArchivedClassroomsAsync()
        {
            return await _context.ArchivedClassrooms
                .Include(ac => ac.ArchivedEnrollments)
                .OrderByDescending(ac => ac.ArchivedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region عمليات الكتابة

        public async Task CreateArchivedClassroomAsync(ArchivedClassroom archivedClassroom)
        {
            await _context.ArchivedClassrooms.AddAsync(archivedClassroom);
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