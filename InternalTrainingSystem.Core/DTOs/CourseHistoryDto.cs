namespace InternalTrainingSystem.Core.DTOs
{
    public class CourseHistoryDto
    {
        public string? FullName { get; set; }
        public string ActionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ActionDate { get; set; }
    }
}
