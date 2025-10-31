using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.DTOs
{
	public class DepartmentDto
	{
		public int DepartmentId { get; set; }

		public string? DepartmentName { get; set; }

		public string? Description { get; set; }
	}

	public class DepartmenCourseAndEmployeeDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int TotalCourses { get; set; }
		public int TotalUsers { get; set; }
		public List<CourseDetailDto>? CourseDetail { get; set; }
		public List<UserProfileDto>? userDetail { get; set; }
	}

	public class DepartmentCourseAndEmployeeInput
	{
		public int Id { get; set; }
		public string? Search { get; set; }
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}
	public class CreateDepartmentDto
	{
		[Required]
		public string? Name { get; set; }
		public string? Description { get; set; }
	}
	public class UpdateDepartmentDto
	{
		[Required]
		public string Name { get; set; } = null!;
		public string? Description { get; set; }
	}
}
