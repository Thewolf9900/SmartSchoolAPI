using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Repositories
{
    public class LectureRepository : ILectureRepository
    {
        private readonly SmartSchoolDbContext _context;

        public LectureRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        #region عمليات القراءة

        public async Task<Lecture?> GetLectureByIdAsync(int lectureId)
        {
             return await _context.Lectures.FindAsync(lectureId);
        }

        public async Task<IEnumerable<Lecture>> GetLecturesByClassroomIdAsync(int classroomId)
        {
            return await _context.Lectures
                .Where(l => l.ClassroomId == classroomId)
                .OrderBy(l => l.LectureOrder)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Lecture?> GetLectureWithMaterialsByIdAsync(int lectureId)
        {
            return await _context.Lectures
                .Include(l => l.Materials)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LectureId == lectureId);
        }

        public async Task<IEnumerable<Lecture>> GetLecturesWithMaterialsAsync(int classroomId)
        {
            return await _context.Lectures
                .Where(l => l.ClassroomId == classroomId)
                .Include(l => l.Materials)
                .Include(l => l.LectureQuiz)
                .OrderBy(l => l.LectureOrder)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<Classroom?> GetClassroomForLectureAsync(int lectureId)
        {
            return await _context.Lectures
                .Where(l => l.LectureId == lectureId)
                .Select(l => l.Classroom)
                .FirstOrDefaultAsync();
        }
        #endregion

        #region عمليات الكتابة

        public void CreateLecture(Lecture lecture)
        {
            _context.Lectures.Add(lecture);
        }

        public void UpdateLecture(Lecture lecture)
        {
           
        }

        public void DeleteLecture(Lecture lecture)
        {
            _context.Lectures.Remove(lecture);
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