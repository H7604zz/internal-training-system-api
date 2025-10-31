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
        public int TotalMembers { get; set; } 
        public List<ClassEmployeeDto> Employees { get; set; } = new List<ClassEmployeeDto>();
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Status { get; set; }
    }

    public class ClassEmployeeDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public int? AbsentNumberDay { get; set; }
    }

    public class CreateClassRequestDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng tối đa phải lớn hơn 0")]
        public int MaxMembers { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng lớp cần mở phải lớn hơn 0")]
        public int NumberOfClasses { get; set; }
    }

    public class GetAllClassesRequest
    {
        public string? Search { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    // Schedule DTOs ----------------------
    public class CreateWeeklyScheduleRequest
    {
        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public string MentorId { get; set; } = string.Empty;

        public DateTime StartWeek { get; set; }
        public int NumberOfWeeks { get; set; }

        public List<WeeklyScheduleItemRequestDto> WeeklySchedules { get; set; } = new();
    }

    public class WeeklyScheduleItemRequestDto
    {
        public string DayOfWeek { get; set; } = ""; // Monday, Tuesday,...
        public string? Description { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
    }

    public class ScheduleItemResponseDto
    {
        public int ScheduleId { get; set; }
        public int? ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? MentorId { get; set; }
        public string Mentor { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
    }

    public class ClassScheduleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ScheduleItemResponseDto> Schedules { get; set; } = new();
    }
}
