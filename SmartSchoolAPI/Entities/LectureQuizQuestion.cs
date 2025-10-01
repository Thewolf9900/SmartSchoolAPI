using SmartSchoolAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("lecture_quiz_questions")]
    public class LectureQuizQuestion
    {
        [Key]
        [Column("lecture_quiz_question_id")]
        public int LectureQuizQuestionId { get; set; }

         [Column("text")]
        public string? Text { get; set; }

         [Column("image_url")]
        [StringLength(512)]
        public string? ImageUrl { get; set; }

        [Column("image_public_id")]
        [StringLength(255)]
        public string? ImagePublicId { get; set; }
        [Required]
        [Column("question_type")]
        public QuestionType QuestionType { get; set; }

        // --- Foreign Key ---
        [Column("lecture_quiz_id")]
        public int LectureQuizId { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("LectureQuizId")]
        public LectureQuiz LectureQuiz { get; set; }
        public ICollection<LectureQuizQuestionOption> Options { get; set; } = new List<LectureQuizQuestionOption>();
    }
}