namespace UJStudentGorvenanceStudentWeb.Models
{
    public class DocumentDetailDto
    {
        public string DocumentId { get; set; }
        public int? DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string DocumentStatus { get; set; } = null!;
        public string? DocumentBase64 { get; set; }
        public List<string> DeclineReasons { get; set; } = new List<string>();
    }

}
