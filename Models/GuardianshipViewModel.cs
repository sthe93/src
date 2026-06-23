namespace UJStudentGorvenanceStudentWeb.Models
{
    public class GuardianshipViewModel
    {
        public int GuardianshipId { get; set; }
        public string GuardianshipType { get; set; } = null!;
        public bool DisableSurnamePrompt { get; set; }
        public bool IsDeleted { get; set; }
        public int AdministratorId { get; set; }
        public List<DocumentTypeViewModel> GuardianDocTypes { get; set; } = [];
    }
}
