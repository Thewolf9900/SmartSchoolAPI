using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSchoolAPI.Entities
{
    [Table("graduation_certificates")]
    public class GraduationCertificate
    {
        [Key]
        [Column("graduation_id")]
        public int GraduationId { get; set; }

        [Required]
        [Column("file_name")]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [Column("file_type")]
        [StringLength(100)]
        public string FileType { get; set; } = string.Empty;

        [Required]
        [Column("certificate_data")]
        public byte[] CertificateData { get; set; } = System.Array.Empty<byte>();

        // --- Navigation Property ---
        [ForeignKey("GraduationId")]
        public Graduation? Graduation { get; set; }
    }
}