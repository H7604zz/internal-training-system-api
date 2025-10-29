using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.DTOs
{
	public class DepartmentDto
	{
		public int DepartmentId { get; set; }

		public string? DepartmentName { get; set; }
	}
	public class DepartmenDetailsDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }
	}
	public class DepartmentInputDto
	{
		public string? Search { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
	}
	public class CreateDepartmentDto
	{
		[Required]
		public string Name { get; set; }
		public string? Description { get; set; }
	}
	public class UpdateDepartmentDto
	{
		public string? Name { get; set; }
		public string? Description { get; set; }
	}
}
