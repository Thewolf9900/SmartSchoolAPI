using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("question_options")]
    public class QuestionOption
    {
        [Key]
        [Column("question_option_id")]
        public int QuestionOptionId { get; set; }

        [Required]
        [Column("text")]
        public string Text { get; set; }

        [Required]
        [Column("is_correct")]
        public bool IsCorrect { get; set; }

        // --- Foreign Key ---
        [Column("question_id")]
        public int QuestionId { get; set; }

        // --- Navigation Property ---
        [ForeignKey("QuestionId")]
        public Question Question { get; set; }
    }
}