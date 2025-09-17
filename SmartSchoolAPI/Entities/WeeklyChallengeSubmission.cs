using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("weekly_challenge_submissions")]
    public class WeeklyChallengeSubmission
    {
        [Key]
        [Column("weekly_challenge_submission_id")]
        public int WeeklyChallengeSubmissionId { get; set; }

        [Column("year")]
        public int Year { get; set; }

        [Column("week_of_year")]
        public int WeekOfYear { get; set; }

        [Column("score")]
        public int Score { get; set; }

        [Column("time_taken_seconds")]
        public int TimeTakenSeconds { get; set; }

        [Column("submitted_at")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // --- Foreign Keys ---
        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("course_id")]
        public int CourseId { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("StudentId")]
        public User Student { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }
    }
}