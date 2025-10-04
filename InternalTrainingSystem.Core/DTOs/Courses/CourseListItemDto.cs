namespace InternalTrainingSystem.Core.DTOs.Courses
{
    public class CourseListItemDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CourseCategoryId { get; set; }
        public string CourseCategoryName { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Level { get; set; } = "Beginner";
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
