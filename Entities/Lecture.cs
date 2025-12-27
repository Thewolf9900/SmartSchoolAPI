using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("lectures")]
    public class Lecture
    {
        [Key]
        [Column("lecture_id")]
        public int LectureId { get; set; }

        [Required]
        [Column("title")]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("lecture_order")]
        public int LectureOrder { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // --- Foreign Key ---
        [Required]
        [Column("classroom_id")]
        public int ClassroomId { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("ClassroomId")]
        public Classroom? Classroom { get; set; }

        public ICollection<Material> Materials { get; set; } = new List<Material>();
        public LectureQuiz? LectureQuiz { get; set; }

    }
}