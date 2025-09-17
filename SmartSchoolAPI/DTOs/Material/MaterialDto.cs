namespace SmartSchoolAPI.DTOs.Material
{
    public class MaterialDto
    {
        public int MaterialId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string MaterialType { get; set; } = string.Empty;// "File", "Link", etc.
        public string? Url { get; set; }
        public string? OriginalFilename { get; set; }
        public long? FileSize { get; set; }
        public DateTime UploadedAt { get; set; }    
    }
}
