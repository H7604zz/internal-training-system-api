namespace InternalTrainingSystem.Core.DTOs
{
    public class MarkFinalDTO
    {
        public class MarkFinalRequest
        {
            public string UserId { get; set; } = string.Empty;
            public double? Score { get; set; }
        }
    }
}
