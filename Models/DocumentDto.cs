namespace UJStudentGorvenanceStudentWeb.Models
{
    public class DocumentDto
    {
       
        public int DocumentId  {get; set;}
        public string Document1 { get; set; }
        public string FileName { get; set; }
        public string DocumentStatus { get; set; }
        public bool IsReUpload { get; set; }
        public bool Reupload { get; set; }
        public int? DocumentTypeId { get; set; }
        public string? LastUpdatedBy { get; set; }

    }
    
}
