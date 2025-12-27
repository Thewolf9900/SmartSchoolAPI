using SmartSchoolAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("questions")]
    public class Question
    {
        [Key]
        [Column("question_id")]
        public int QuestionId { get; set; }

        [Column("text")]
        public string? Text { get; set; }  

        [Column("image_url")]
        [StringLength(512)]
        public string? ImageUrl { get; set; }

        [Column("image_public_id")]
        [StringLength(255)]
        public string? ImagePublicId { get; set; } = null;

        [Required]
        [Column("question_type", TypeName = "varchar(50)")]
        public QuestionType QuestionType { get; set; }

        [Required]
        [Column("difficulty_level", TypeName = "varchar(50)")]
        public DifficultyLevel DifficultyLevel { get; set; }

        [Required]
        [Column("status", TypeName = "varchar(50)")]
        public QuestionStatus Status { get; set; } = QuestionStatus.Pending;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        [Column("course_id")]
        public int CourseId { get; set; }

        [Column("created_by_id")]
        public int CreatedById { get; set; }

        [Column("reviewed_by_id")]
        public int? ReviewedById { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        [ForeignKey("CreatedById")]
        public User CreatedBy { get; set; }

        [ForeignKey("ReviewedById")]
        public User? ReviewedBy { get; set; }

        public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }
}