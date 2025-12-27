using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("materials")]
    public class Material
    {
        [Key]
        [Column("material_id")]
        public int MaterialId { get; set; }

        [Column("public_id")]
        [StringLength(255)]
        public string? PublicId { get; set; }

        [Required]
        [Column("title")]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("material_type")]
        [StringLength(50)]
        public string MaterialType { get; set; } = string.Empty;

        [Required]
        [Column("url")]
        [StringLength(512)]
        public string Url { get; set; } = string.Empty;

        [Column("original_filename")]
        [StringLength(255)]
        public string? OriginalFilename { get; set; }

        [Column("file_type")]
        [StringLength(100)]
        public string? FileType { get; set; }

        [Column("file_size")]
        public long? FileSize { get; set; }

        [Column("uploaded_at")]
        public DateTime UploadedAt { get; set; }

        // --- Foreign Keys ---
        [Column("lecture_id")]
        public int? LectureId { get; set; }

        [Column("course_id")]
        public int? CourseId { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("LectureId")]
        public Lecture? Lecture { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }
    }
}