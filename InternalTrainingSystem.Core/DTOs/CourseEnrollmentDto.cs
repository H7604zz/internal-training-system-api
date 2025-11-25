namespace InternalTrainingSystem.Core.DTOs
{

    public class EnrollmentStatusUpdateRequest
    {
        public bool IsConfirmed { get; set; }
        public string? Reason { get; set; }
    }
    
    public class ConfirmReasonRequest
    {
        public string UserId { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
