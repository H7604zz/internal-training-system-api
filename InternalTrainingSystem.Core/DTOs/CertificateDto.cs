namespace InternalTrainingSystem.Core.DTOs
{

    public class CertificateResponse
    {
        public int CertificateId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CertificateName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
