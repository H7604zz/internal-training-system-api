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
        public List<ClassStudentDto> Students { get; set; } = new List<ClassStudentDto>();
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ClassStudentDto
    {
        public string StudentId { get; set; } = string.Empty;
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
    }

    public class CreateClassRequestDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public List<string> StaffIds { get; set; } = new List<string>();

        [Required]
        public string MentorId { get; set; } = string.Empty;
    }

    public class CreateClassesDto
    {
        [Required]
        public List<CreateClassRequestDto> Classes { get; set; } = new List<CreateClassRequestDto>();
    }
}
