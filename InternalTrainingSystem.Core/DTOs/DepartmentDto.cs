using System.ComponentModel.DataAnnotations;
using InternalTrainingSystem.Core.Common;

namespace InternalTrainingSystem.Core.DTOs
{
	public class DepartmentListDto
	{
		public int DepartmentId { get; set; }

		public string? DepartmentName { get; set; }

		public string? Description { get; set; }
	}
	public class DepartmentDetailDto
	{
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? Description { get; set; }
        public PagedResult<UserProfileDto>? UserDetail { get; set; }
    }

	public class DepartmentRequestDto
	{
		[Required]
		public string? Name { get; set; }
		public string? Description { get; set; }
	}

	public class DepartmentDetailRequestDto
	{
		public int DepartmentId { get; set; }
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}

	public class TransferEmployeeDto
	{
		[Required(ErrorMessage = "User ID là bắt buộc")]
		public string UserId { get; set; } = string.Empty;

		[Required(ErrorMessage = "Phòng ban đích là bắt buộc")]
		public int TargetDepartmentId { get; set; }
	}
}
