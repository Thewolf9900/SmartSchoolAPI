using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Repositories
{
    public class LectureQuizRepository : ILectureQuizRepository
    {
        private readonly SmartSchoolDbContext _context;

        public LectureQuizRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        public async Task<LectureQuiz> CreateQuizAsync(LectureQuiz quiz)
        {
            await _context.LectureQuizzes.AddAsync(quiz);
            return quiz;
        }

        public async Task<LectureQuizQuestion> CreateQuestionAsync(LectureQuizQuestion question)
        {
            await _context.LectureQuizQuestions.AddAsync(question);
            return question;
        }

        public async Task<LectureQuiz?> GetQuizByIdAsync(int quizId)
        {
            return await _context.LectureQuizzes.FindAsync(quizId);
        }
 
        public async Task<LectureQuiz?> GetQuizByLectureIdAsync(int lectureId)
        {
            return await _context.LectureQuizzes
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.Options)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.LectureId == lectureId);
        }

        public async Task<LectureQuiz?> GetQuizWithDetailsByIdAsync(int quizId)
        {
            return await _context.LectureQuizzes
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.Options)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.LectureQuizId == quizId);
        }

        public async Task<LectureQuizQuestion?> GetQuestionWithDetailsByIdAsync(int questionId)
        {
            return await _context.LectureQuizQuestions
                .Include(q => q.LectureQuiz)
                  .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.LectureQuizQuestionId == questionId);
        }

        public async Task<IEnumerable<LectureQuizSubmission>> GetSubmissionsForQuizAsync(int quizId)
        {
            return await _context.LectureQuizSubmissions
                .Where(s => s.LectureQuizId == quizId)
                .Include(s => s.Student)
                .OrderByDescending(s => s.Score)
                .ThenBy(s => s.SubmittedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public void DeleteQuiz(LectureQuiz quiz)
        {
            _context.LectureQuizzes.Remove(quiz);
        }

        public void DeleteQuestion(LectureQuizQuestion question)
        {
            _context.LectureQuizQuestions.Remove(question);
        }
        public async Task<LectureQuizSubmission> GetSubmissionAsync(int quizId, int studentId)
        {
            return await _context.LectureQuizSubmissions
           .FirstOrDefaultAsync(s => s.LectureQuizId == quizId && s.StudentId == studentId);
        }
        public async Task<List<LectureQuizSubmission>> GetSubmissionsByStudentAndQuizzesAsync(int studentId, IEnumerable<int> quizIds)
        {
            return await _context.LectureQuizSubmissions
                .Where(s => s.StudentId == studentId && quizIds.Contains(s.LectureQuizId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<LectureQuizSubmission?> GetSubmissionForReviewAsync(int submissionId)
        {
            return await _context.LectureQuizSubmissions
                 .Include(s => s.LectureQuiz)
                    .ThenInclude(lq => lq.Questions)
                        .ThenInclude(q => q.Options)

                // جلب إجابات الطالب
                .Include(s => s.StudentAnswers)
                    .ThenInclude(sa => sa.SelectedOption)

                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.LectureQuizSubmissionId == submissionId);
        }

        public async Task AddSubmissionAsync(LectureQuizSubmission submission)
        {
            await _context.LectureQuizSubmissions.AddAsync(submission);
        }
        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}