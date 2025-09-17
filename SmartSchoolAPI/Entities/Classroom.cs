using SmartSchoolAPI.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("classrooms")]
    public class Classroom
    {
        [Key]
        [Column("classroom_id")]
        public int ClassroomId { get; set; }

        [Required]
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("capacity")]
        public int Capacity { get; set; }

        [Required]
        [Column("status")]
        public ClassroomStatus Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // --- Foreign Keys ---
        [Required]
        [Column("course_id")]
        public int CourseId { get; set; }

        [Column("teacher_id")]
        public int? TeacherId { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        [ForeignKey("TeacherId")]
        public User? Teacher { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Lecture> Lectures { get; set; } = new List<Lecture>();
        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
    }
}