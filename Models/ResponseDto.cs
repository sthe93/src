namespace UJStudentGorvenanceStudentWeb.Models
{
    public class ResponseDto
    {
        public string? Message { get; set; } = null!;
        public object Data { get; set; } = null!;
        public bool IsValid { get; set; }
        
    }
}
