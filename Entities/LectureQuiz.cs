using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

  
namespace SmartSchoolAPI.Entities
{
    [Table("lecture_quizzes")]
    public class LectureQuiz
    {
        [Key]
        [Column("lecture_quiz_id")]
        public int LectureQuizId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("title")]
        public string Title { get; set; }

        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- Foreign Key ---
        [Column("lecture_id")]
        public int LectureId { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("LectureId")]
        public Lecture Lecture { get; set; }

        public ICollection<LectureQuizQuestion> Questions { get; set; } = new List<LectureQuizQuestion>();
        public ICollection<LectureQuizSubmission> Submissions { get; set; } = new List<LectureQuizSubmission>();
    }
}