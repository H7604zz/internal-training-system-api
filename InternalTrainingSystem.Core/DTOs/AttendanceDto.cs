namespace InternalTrainingSystem.Core.DTOs
{
    public class AttendanceRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string? Status { get; set; }
    }

    public class AttendanceResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CheckOutTime { get; set; }
    }
}
