using UJStudentGorvenanceStudentWeb.Models;

namespace StudentGovernanceStudentWeb.Models
{
    public class EditDocumentDto
    {
        public int ApplicationId { get; set; }
        public int DocumentId { get; set; }
        public List<DocumentDto> Documents { get; set; }
    }
}
