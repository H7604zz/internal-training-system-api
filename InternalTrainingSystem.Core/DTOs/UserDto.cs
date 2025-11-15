using InternalTrainingSystem.Core.Configuration.Constants;
using InternalTrainingSystem.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.DTOs
{
    public class EligibleStaffResponse
    {
        public string? Id { get; set; }
        public string? EmployeeId { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }

        public string? Department { get; set; }

        public string? Position { get; set; }
        public string? Status { get; set; }

        public string? Reason { get; set; }
    }

    public class StaffConfirmCourseResponse
    {
        public string? Id { get; set; }
        public string? EmployeeId { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }

        public string? Department { get; set; }

        public string? Position { get; set; }
        public string? Status { get; set; }
    }

    public class UserDetailResponse
    {
        public string Id { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Position { get; set; }
    }
    public class CreateUserDto
    {
        [Required, StringLength(100)]
        public string? EmployeeId { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Phone, StringLength(30)]
        public string? Phone { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        // Chỉ một Role duy nhất
        [StringLength(256)]
        public string? RoleName { get; set; } = "Staff";
    }


    public class UpdateUserDto
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? EmployeeId { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        public DateTime? HireDate { get; set; }

        public bool IsActive { get; set; } = true;

        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UpdateProfileDto
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }
    }

    public class AdminResetPasswordDto
    {
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UserSearchDto
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class UserCourseSummaryDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;

        // Attendance
        public int TotalSessions { get; set; }
        public int AbsentDays { get; set; }
        public double AttendanceRate { get; set; }

        // Course Enrollment Info
        public string Status { get; set; } = EnrollmentConstants.Status.InProgress;
        public double? Score { get; set; } 
    }

    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();

        public static ApiResponseDto<T> SuccessResult(T data, string message = "")
        {
            return new ApiResponseDto<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponseDto<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponseDto<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
