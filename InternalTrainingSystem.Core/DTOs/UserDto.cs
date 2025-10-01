using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.DTOs
{
    public class StaffWithoutCertificateResponse
    {
        public string? EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public string? Department { get; set; }

        public string? Position { get; set; }
        public string? Status { get; set; }
    }

    public class StaffConfirmCourseResponse
    {
        public string? EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public string? Department { get; set; }

        public string? Position { get; set; }
        public string? Status { get; set; }
    }
}
