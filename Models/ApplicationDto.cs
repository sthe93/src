namespace UJStudentGorvenanceStudentWeb.Models
{
    public class ApplicationDto
    {

        public int ApplicationId { get; set; }
        public string StudentNumber { get; set; }
        public string? StudentIdNumber { get; set; }
        public string? ApplicationYear { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? StudentName { get; set; }
        public DateTime UpdatedOnDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime LastUpdated { get; set; }
        public string ApplicationType { get; set; }
        public string DisplayApplicationType
        {
            get
            {
                return ApplicationType == "Student Representative Council Trust Fund"
                    ? "Student Representative Council Inclusivity Fund"
                    : ApplicationType;
            }
        }

        public string? CourseCode { get; set; }
        public string? AcademicDecStatus { get; set; }
        public int? AdministratorId { get; set; }
        public bool IsSurnamesTheSame { get; set; }
        public string GuardianshipType { get; set; }
        public bool? AllowManage { get; set; }
        public string? ApplicationBlock { get; set; }
        public string? CourseDescription { get; set; }
        public string? Faculty { get; set; }
        public string? Nationality { get; set; }
        public string? SentBackReason { get; set; }
        public string? StatusUpdateReason { get; set; }
        public byte[]? SupportingDocuments { get; set; }
        public List<DocumentDto>? Documents { get; set; }
    }
}
