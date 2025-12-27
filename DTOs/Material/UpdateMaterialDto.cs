using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.Material
{
    public class UpdateMaterialDto
    {
        [Required]
        public string Title { get; set; }=string.Empty;
        public string? Description { get; set; }
    }
}