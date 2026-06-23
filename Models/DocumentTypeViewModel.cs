namespace UJStudentGorvenanceStudentWeb.Models
{
    public class DocumentTypeViewModel
    {
        public int DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = null!;
        public bool IsRequired { get; set; }
        public bool IsChecked { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsHidden { get; set; }
    }
}
