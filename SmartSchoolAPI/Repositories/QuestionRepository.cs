using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly SmartSchoolDbContext _context;

        public QuestionRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        public async Task CreateQuestionAsync(Question question)
        {
            await _context.Questions.AddAsync(question);
        }
        public async Task<IEnumerable<Question>> GetQuestionsByCourseAsync(int courseId)
        {
            return await _context.Questions
                .Where(q => q.CourseId == courseId)
                .Include(q => q.Options)
                .Include(q => q.CreatedBy)
                .Include(q => q.ReviewedBy)
                .OrderByDescending(q => q.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<Question?> GetQuestionByIdAsync(int questionId)
        {
            return await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Options)  
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);
        }

        public async Task<IEnumerable<Question>> GetPendingSuggestionsByAuthorAsync(int courseId, int authorId)
        {
            return await _context.Questions
                .Where(q => q.CourseId == courseId &&
                             q.CreatedById == authorId &&
                             q.Status == Enums.QuestionStatus.Pending)
                .Include(q => q.Options)
                .OrderByDescending(q => q.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<List<Question>> GetRandomApprovedQuestionsForCourseAsync(int courseId, int count)
        {
            return await _context.Questions
                .Where(q => q.CourseId == courseId && q.Status == QuestionStatus.Approved)
                .Include(q => q.Options)
                .OrderBy(q => EF.Functions.Random()) 
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }
        public void DeleteQuestion(Question question)
        {
            _context.Questions.Remove(question);
        }


        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}