namespace InternalTrainingSystem.Core.DTOs
{
    public class CourseListDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class GetCoursesByIdentifiersRequest
    {
        public List<string> Identifiers { get; set; } = new List<string>();
    }
}
