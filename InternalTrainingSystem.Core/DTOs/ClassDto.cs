using InternalTrainingSystem.Core.Common.Constants;
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
        public int MaxStudents { get; set; }
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
        public int? AbsentNumberDay { get; set; } = 0;
        public double? ScoreFinal { get; set; } = 0;
    }

    public class CreateClassRequestDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng lớp cần mở phải lớn hơn 0")]
        public int NumberOfClasses { get; set; }

        public string? Description { get; set; }
    }

    public class GetAllClassesRequest
    {
        public string? Search { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class ClassListDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string MentorId { get; set; } = string.Empty;
        public string? MentorName { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Status { get; set; }
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
        public DateTime Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
        public string? OnlineLink { get; set; }
        public string AttendanceStatus { get; set; } = AttendanceConstants.Status.NotYet;
    }

    public class RescheduleRequest
    {
        [Required]
        public string NewDayOfWeek { get; set; } = string.Empty;
        [Required]
        public DateTime NewDate { get; set; }
        [Required]
        public TimeSpan NewStartTime { get; set; }
        [Required]
        public TimeSpan NewEndTime { get; set; }
        
        [Required] 
        public string NewLocation { get; set; } = string.Empty;
    }

    //class swap dto

    public class SwapClassRequest
    {
        public string EmployeeIdFrom { get; set; } = string.Empty;
        public int ClassIdFrom { get; set; }

        public string EmployeeIdTo { get; set; } = string.Empty;
        public int ClassIdTo { get; set; }
    }

    public class RespondSwapRequest
    {
        public int SwapRequestId { get; set; }
        public bool Accepted { get; set; }
    }

    public class ClassSwapDto
    {
        public string RequesterName { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public string FromClassName { get; set; } = string.Empty;
        public string ToClassName { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
    }
}
