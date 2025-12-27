// في ملف: SmartSchoolAPI/Repositories/WeeklyChallengeRepository.cs

using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Repositories
{
    public class WeeklyChallengeRepository : IWeeklyChallengeRepository
    {
        private readonly SmartSchoolDbContext _context;

        public WeeklyChallengeRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        public async Task<WeeklyChallengeSubmission?> GetSubmissionAsync(int studentId, int courseId, int year, int weekOfYear)
        {
            return await _context.WeeklyChallengeSubmissions
                .FirstOrDefaultAsync(s =>
                    s.StudentId == studentId &&
                    s.CourseId == courseId &&
                    s.Year == year &&
                    s.WeekOfYear == weekOfYear);
        }

        public async Task AddSubmissionAsync(WeeklyChallengeSubmission submission)
        {
            await _context.WeeklyChallengeSubmissions.AddAsync(submission);
        }

        public async Task<List<WeeklyChallengeSubmission>> GetLeaderboardAsync(int courseId, int year, int weekOfYear)
        {
            return await _context.WeeklyChallengeSubmissions
                .Include(s => s.Student)  
                .Where(s =>
                    s.CourseId == courseId &&
                    s.Year == year &&
                    s.WeekOfYear == weekOfYear)
                .OrderByDescending(s => s.Score) 
                 .ThenBy(s => s.TimeTakenSeconds)
                 .AsNoTracking()
                .ToListAsync();
        }
    }
}