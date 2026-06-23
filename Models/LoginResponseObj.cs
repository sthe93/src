namespace UJStudentGorvenanceStudentWeb.Models
{
    public class LoginResponseObj : ResponseDto
    {
        public bool HasApplications { get; set; }
        public List<string> ApplicationTypes { get; set; }
    }
}
