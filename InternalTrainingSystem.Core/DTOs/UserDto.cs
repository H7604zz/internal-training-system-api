using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.DTOs
{
    public class EligibleStaffResponse
    {
        public string? EmployeeId { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }

        public string? Department { get; set; }

        public string? Position { get; set; }
        public string? Status { get; set; }

        public string? Reason { get; set; }
    }

    public class StaffConfirmCourseResponse
    {
        public string Id { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }

        public string? Department { get; set; }

        public string? Position { get; set; }
        public string? Status { get; set; }
    }

    public class MentorResponse
    {
        public string Id { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Position { get; set; }
    }

    public class StaffResponse
    {
        public string Id { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Position { get; set; }
    }
}
