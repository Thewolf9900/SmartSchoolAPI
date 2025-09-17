using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("lecture_quiz_submissions")]
    public class LectureQuizSubmission
    {
        [Key]
        [Column("lecture_quiz_submission_id")]
        public int LectureQuizSubmissionId { get; set; }

        [Column("score")]
        public int Score { get; set; }

        [Column("total_questions")]
        public int TotalQuestions { get; set; }

        [Column("submitted_at")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // --- Foreign Keys ---
        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("lecture_quiz_id")]
        public int LectureQuizId { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("StudentId")]
        public User Student { get; set; }

        [ForeignKey("LectureQuizId")]
        public LectureQuiz LectureQuiz { get; set; }

         public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }
}