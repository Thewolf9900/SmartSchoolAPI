using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Repositories
{
    public class MaterialRepository : IMaterialRepository
    {
        private readonly SmartSchoolDbContext _context;

        public MaterialRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        #region عمليات القراءة

        public async Task<Material?> GetMaterialByIdAsync(int materialId)
        {
             return await _context.Materials
               .Include(m => m.Lecture)
               .FirstOrDefaultAsync(m => m.MaterialId == materialId);
        }

        public async Task<IEnumerable<Material>> GetMaterialsForCourseAsync(int courseId)
        {
            return await _context.Materials
                .Where(m => m.CourseId == courseId && m.LectureId == null)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Material?> GetMaterialForStudentDownloadAsync(int materialId)
        {
            return await _context.Materials
                .Include(m => m.Lecture)
                    .ThenInclude(l => l.Classroom)
                        .ThenInclude(c => c.Enrollments)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaterialId == materialId);
        }

        public async Task<Material?> GetMaterialWithDeepDetailsAsync(int materialId)
        {
            return await _context.Materials
          .Include(m => m.Course)
          .Include(m => m.Lecture)
              .ThenInclude(l => l.Classroom)
                  .ThenInclude(c => c.Enrollments)
         .AsNoTracking()
         .FirstOrDefaultAsync(m => m.MaterialId == materialId);
        }

        #endregion

        #region عمليات الكتابة

        public async Task CreateMaterialAsync(Material material)
        {
            await _context.Materials.AddAsync(material);
        }

        public void UpdateMaterial(Material material)
        {
         }

        public void DeleteMaterial(Material material)
        {
            _context.Materials.Remove(material);
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