namespace InternalTrainingSystem.Core.DTOs
{
    public class CourseHistoryDto
    {
        public int HistoryId { get; set; }
        public int CourseId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string ActionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ActionDate { get; set; }
    }
}
