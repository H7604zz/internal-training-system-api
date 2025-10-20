namespace InternalTrainingSystem.Core.DTOs
{

    public class EnrollmentStatusUpdateRequest
    {
        public bool IsConfirmed { get; set; }
        public string? Reason { get; set; }
    }
}
