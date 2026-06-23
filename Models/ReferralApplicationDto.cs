namespace UJStudentGorvenanceStudentWeb.Models
{
    public class ReferralApplicationDto
    {
        public int ReferralApplicationId { get; set; }
        public string StaffNames { get; set; } = null!;
        public string StaffNumber { get; set; } = null!;
        public string? StudentNames { get; set; }
        public string StudentNumber { get; set; } = null!;
        public string? Status { get; set; } 
        public string? CreatedBy { get; set; }
        public DateTime UpdatedOnDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? ApplicationType { get; set; }
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
        public string? CourseDescription { get; set; }
        public string? Faculty { get; set; }
        public string? Nationality { get; set; }
        public int AcademicYear { get; set; }
        public string? IdPassportNumber { get; set; }
        public string? Motivation { get; set; }
        public string? Reason { get; set; }

        public List<ReferralApplicationsDocumentDto> Documents { get; set; } = null!;
    }
}
