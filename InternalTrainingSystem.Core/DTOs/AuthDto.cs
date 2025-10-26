using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }

    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserProfileDto? User { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? Roles { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ChangePasswordRequestDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ApiResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public static ApiResponseDto SuccessResult(object? data = null, string message = "")
        {
            return new ApiResponseDto
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponseDto ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponseDto
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}