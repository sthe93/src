namespace UJStudentGorvenanceStudentWeb.Models
{
    public class ReferralApplicationsDocumentDto
    {
        public string DocumentName { get; set; } = null!;
        public string DocumentData { get; set; } = null!;
        public string DocumentTypeName { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
