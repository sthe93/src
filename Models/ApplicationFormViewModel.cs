namespace UJStudentGorvenanceStudentWeb.Models
{
    public class ApplicationFormViewModel
    {
        public string? CurrentStudentNumber { get; set; }
        public string? CurrentStudentName { get; set; }
        public string? CurrentStudentIdNumber { get; set; }
        public List<GuardianshipViewModel>? GuardianshipTypes { get; set; }
        public ApplicationDto Application { get; set; }
    }
}

