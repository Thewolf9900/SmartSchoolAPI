using SmartSchoolAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    public interface IQuestionRepository
    {
        Task CreateQuestionAsync(Question question);
        Task<IEnumerable<Question>> GetQuestionsByCourseAsync(int courseId);
        Task<Question?> GetQuestionByIdAsync(int questionId);
        Task<IEnumerable<Question>> GetPendingSuggestionsByAuthorAsync(int courseId, int authorId);
        Task<List<Question>> GetRandomApprovedQuestionsForCourseAsync(int courseId, int count);

        void DeleteQuestion(Question question);
        Task<bool> SaveChangesAsync();
    }
}