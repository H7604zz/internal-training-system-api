using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.DTOs
{
	public class DepartmentListDto
	{
		public int DepartmentId { get; set; }

		public string? DepartmentName { get; set; }

		public string? Description { get; set; }
	}

	public class DepartmenDetailDto
	{
		public int Id { get; set; }
		public string? Name { get; set; }
		public int TotalCourses { get; set; }
		public int TotalUsers { get; set; }
		public List<CourseDetailDto>? CourseDetail { get; set; }
		public List<UserProfileDto>? userDetail { get; set; }
	}

	public class DepartmentRequestDto
	{
		[Required]
		public string? Name { get; set; }
		public string? Description { get; set; }
	}
}
