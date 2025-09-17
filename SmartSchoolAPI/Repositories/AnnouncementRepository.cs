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
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly SmartSchoolDbContext _context;

        public AnnouncementRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        #region دوال عامة وخاصة بالمدير

        public async Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync()
        {
            return await GetFullAnnouncementQuery()
                .AsNoTracking()
                .OrderByDescending(a => a.PostedAt)
                .ToListAsync();
        }

        public async Task<Announcement?> GetAnnouncementByIdAsync(int announcementId)
        {
            return await GetFullAnnouncementQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AnnouncementId == announcementId);
        }

        public async Task CreateAnnouncementAsync(Announcement announcement)
        {
            await _context.Announcements.AddAsync(announcement);
        }

        public void DeleteAnnouncement(Announcement announcement)
        {
            _context.Announcements.Remove(announcement);
        }

        #endregion

        #region دوال خاصة بالمدرس

        public async Task<IEnumerable<Announcement>> GetLatestAnnouncementsForClassroomAsync(int classroomId, int count)
        {
            return await _context.Announcements
                .Where(a => a.ClassroomId == classroomId)
                .AsNoTracking()
                .OrderByDescending(a => a.PostedAt)
                .Take(count)
                .ToListAsync();
        }

        #endregion

        #region دوال خاصة بالطالب

        public async Task<IEnumerable<Announcement>> GetAnnouncementsForStudentAsync(int programId, List<int> courseIds, List<int> classroomIds)
        {
            return await GetFullAnnouncementQuery()
                .Where(a =>
                    a.TargetScope == AnnouncementScope.GLOBAL ||
                    (a.TargetScope == AnnouncementScope.PROGRAM && a.AcademicProgramId == programId) ||
                    (a.TargetScope == AnnouncementScope.COURSE && a.CourseId.HasValue && courseIds.Contains(a.CourseId.Value)) ||
                    (a.TargetScope == AnnouncementScope.CLASSROOM && a.ClassroomId.HasValue && classroomIds.Contains(a.ClassroomId.Value))
                )
                .AsNoTracking()
                .OrderByDescending(a => a.PostedAt)
                .ToListAsync();
        }
        #endregion

        #region إدارة حفظ البيانات

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        #endregion

        #region دوال مساعدة خاصة

        private IQueryable<Announcement> GetFullAnnouncementQuery()
        {
            return _context.Announcements
                .Include(a => a.AcademicProgram)
                .Include(a => a.Course)
                .Include(a => a.Classroom)
                    .ThenInclude(c => c.Course);
        }

        #endregion
    }
}