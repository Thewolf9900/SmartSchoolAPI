using SmartSchoolAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    public interface ILectureQuizRepository
    {
        Task<LectureQuiz> CreateQuizAsync(LectureQuiz quiz);
        Task<LectureQuizQuestion> CreateQuestionAsync(LectureQuizQuestion question);
        Task<LectureQuiz?> GetQuizByIdAsync(int quizId);
        Task<LectureQuiz?> GetQuizByLectureIdAsync(int lectureId);

        Task<LectureQuiz?> GetQuizWithDetailsByIdAsync(int quizId);
        Task<LectureQuizQuestion?> GetQuestionWithDetailsByIdAsync(int questionId);
        Task<IEnumerable<LectureQuizSubmission>> GetSubmissionsForQuizAsync(int quizId);
        void DeleteQuiz(LectureQuiz quiz);
        void DeleteQuestion(LectureQuizQuestion question);

         Task<LectureQuizSubmission> GetSubmissionAsync(int quizId, int studentId);
        Task<List<LectureQuizSubmission>> GetSubmissionsByStudentAndQuizzesAsync(int studentId, IEnumerable<int> quizIds);
        Task<LectureQuizSubmission?> GetSubmissionForReviewAsync(int submissionId);

        Task AddSubmissionAsync(LectureQuizSubmission submission);
        Task<bool> SaveChangesAsync();
    }
}