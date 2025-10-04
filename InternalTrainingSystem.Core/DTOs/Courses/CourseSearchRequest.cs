namespace InternalTrainingSystem.Core.DTOs.Courses
{
    public class CourseSearchRequest
    {
        public string? Q { get; set; }

        public string? Category { get; set; }

        public List<string>? Categories { get; set; }

        public int? CategoryId { get; set; }

        public bool? IsActive { get; set; }
        public string? Level { get; set; }
        public int? DurationFrom { get; set; }
        public int? DurationTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? Sort { get; set; } = "-CreatedDate";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
