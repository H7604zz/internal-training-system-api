using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.DTOs
{
    public class ClassDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public string MentorId { get; set; } = string.Empty;
        public string? MentorName { get; set; }
        public List<ClassEmployeeDto> Employees { get; set; } = new List<ClassEmployeeDto>();
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ClassEmployeeDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    public class CreateClassRequestDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public List<string> EmployeeIds { get; set; } = new List<string>();

        [Required]
        public string MentorId { get; set; } = string.Empty;
    }

    public class CreateClassesDto
    {
        [Required]
        public List<CreateClassRequestDto> Classes { get; set; } = new List<CreateClassRequestDto>();
    }

    public class GetAllClassesRequest
    {
        public string? Search { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
