// In SmartSchoolAPI/Entities/StudentAnswer.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("student_answers")]
    public class StudentAnswer
    {
        [Key]
        [Column("student_answer_id")]
        public int StudentAnswerId { get; set; }

        // --- Foreign Keys ---

        // يربط هذه الإجابة بالتقديم العام للاختبار
        [Column("lecture_quiz_submission_id")]
        public int LectureQuizSubmissionId { get; set; }

        // يحدد السؤال الذي تم الإجابة عليه
        [Column("lecture_quiz_question_id")]
        public int LectureQuizQuestionId { get; set; }

        // يحدد الخيار الذي اختاره الطالب
        [Column("selected_option_id")]
        public int SelectedOptionId { get; set; }


        // --- Navigation Properties ---

        [ForeignKey("LectureQuizSubmissionId")]
        public LectureQuizSubmission Submission { get; set; }

        [ForeignKey("LectureQuizQuestionId")]
        public LectureQuizQuestion Question { get; set; }

        [ForeignKey("SelectedOptionId")]
        public LectureQuizQuestionOption SelectedOption { get; set; }
    }
}