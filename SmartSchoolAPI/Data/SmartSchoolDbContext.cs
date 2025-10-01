using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;

namespace SmartSchoolAPI.Data
{
    public class SmartSchoolDbContext : DbContext
    {
        public SmartSchoolDbContext(DbContextOptions<SmartSchoolDbContext> options) : base(options)
        {
        }

        public DbSet<AcademicProgram> AcademicPrograms { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Lecture> Lectures { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Graduation> Graduations { get; set; }
        public DbSet<FailedStudent> FailedStudents { get; set; }
        public DbSet<ArchivedClassroom> ArchivedClassrooms { get; set; }
        public DbSet<GraduationCertificate> GraduationCertificates { get; set; }
        public DbSet<ArchivedEnrollment> ArchivedEnrollments { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<LectureQuiz> LectureQuizzes { get; set; }
        public DbSet<LectureQuizQuestion> LectureQuizQuestions { get; set; }
        public DbSet<LectureQuizQuestionOption> LectureQuizQuestionOptions { get; set; }
        public DbSet<LectureQuizSubmission> LectureQuizSubmissions { get; set; }

         public DbSet<StudentAnswer> StudentAnswers { get; set; }  

        public DbSet<WeeklyChallengeSubmission> WeeklyChallengeSubmissions { get; set; }
        public DbSet<ChatConversation> ChatConversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<ProgramRegistration> ProgramRegistrations { get; set; }
        public DbSet<PaymentSettings> PaymentSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
             base.OnModelCreating(modelBuilder);

             modelBuilder.Entity<Question>().Property(q => q.QuestionType).HasConversion<string>();
            modelBuilder.Entity<Question>().Property(q => q.Status).HasConversion<string>();
            modelBuilder.Entity<Question>().Property(q => q.DifficultyLevel).HasConversion<string>();
            modelBuilder.Entity<LectureQuizQuestion>().Property(q => q.QuestionType).HasConversion<string>();

            modelBuilder.Entity<User>().Property(u => u.Role).HasConversion<string>();
            modelBuilder.Entity<Announcement>().Property(a => a.TargetScope).HasConversion<string>();
            modelBuilder.Entity<Classroom>().Property(c => c.Status).HasConversion<string>();
            modelBuilder.Entity<ProgramRegistration>().Property(r => r.Status).HasConversion<string>();


            modelBuilder.Entity<Question>()
                .HasOne(q => q.CreatedBy)
                .WithMany()
                .HasForeignKey(q => q.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.ReviewedBy)
                .WithMany()
                .HasForeignKey(q => q.ReviewedById)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Coordinator)
                .WithMany()
                .HasForeignKey(c => c.CoordinatorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<LectureQuizSubmission>()
                .HasIndex(s => new { s.StudentId, s.LectureQuizId })
                .IsUnique();

            modelBuilder.Entity<WeeklyChallengeSubmission>()
                .HasIndex(s => new { s.StudentId, s.CourseId, s.Year, s.WeekOfYear })
                .IsUnique();
        }
    }
}