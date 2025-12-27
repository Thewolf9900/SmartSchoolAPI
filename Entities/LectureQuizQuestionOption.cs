using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("lecture_quiz_question_options")]
    public class LectureQuizQuestionOption
    {
        [Key]
        [Column("lecture_quiz_question_option_id")]
        public int LectureQuizQuestionOptionId { get; set; }

        [Required]
        [Column("text")]
        public string Text { get; set; }

        [Required]
        [Column("is_correct")]
        public bool IsCorrect { get; set; }

        // --- Foreign Key ---
        [Column("lecture_quiz_question_id")]
        public int LectureQuizQuestionId { get; set; }

        // --- Navigation Property ---
        [ForeignKey("LectureQuizQuestionId")]
        public LectureQuizQuestion LectureQuizQuestion { get; set; }
    }
}