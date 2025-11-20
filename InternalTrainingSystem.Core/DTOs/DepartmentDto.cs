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
		public int PageSize { get; set; } = 30;
	}
}
