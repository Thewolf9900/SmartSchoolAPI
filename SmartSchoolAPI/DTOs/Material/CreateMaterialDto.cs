using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Material
{
    public class CreateMaterialDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }=string.Empty;

        public string? Description { get; set; }

         public IFormFile? File { get; set; }
        public string? Url { get; set; }
    }
}
